using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using EasyCharacterMovement;
using EasyCharacterMovement.Templates.SideScrollerTemplate;

public class SentryTurretAgent : Agent, EnemyInterface
{
    public Vector3 spawn { get; set; }

    public Transform target; // Player's (Target's) Transform
    public Vector3 playerSpawn;
    public MyCharacter player;
    public float rotationSpeed = 5f;

    public GameObject bulletPrefab;
    public GameObject laserPrefab;
    public Transform bulletSpawnPoint;
    public float machineGunBulletCooldown = 0.15f;
    public float machineGunBurstCooldown = 1.5f;
    public float laserChargeTime = 2f;
    public float laserBurstTime = 0.4f;
    public float laserDamage = 8f;
    public float range = 18f;


    private float machineGunBurstTimer;
    private float laserChargeTimer;
    private bool isLaserCharging;

    public float health { get; set; }
    public float maxHealth { get; set; }
    //private float playerHealth = 100;

    public int hitCounter { get; set; }
    public int playerHitCounter = 0;

    public Transform startingTarget;

    private bool playerInSight = false;

    private void Awake()
    {
        if (target.TryGetComponent(out MyCharacter ply))
        {
            player = target.GetComponent<MyCharacter>();
            playerSpawn = player.transform.position;
        }
        else
        {
            player = null;
            //playerSpawn = player.transform.position;
        }

        maxHealth = 45;
        health = maxHealth;
        spawn = transform.position;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((target != null ? (Vector3)target.transform.position : Vector3.zero));
        sensor.AddObservation((Vector3)transform.position);
        sensor.AddObservation((player != null ? player.health : 100)); // if player is not the target, can't access its health so default 100
        sensor.AddObservation(health);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        bool weaponChoice = actions.DiscreteActions[0] > 0.5f; // charges laser if over threshold
        bool canShoot = actions.DiscreteActions[1] > 0.5f; // can shoot if above threshold
        float accuracy = actions.DiscreteActions[2]; // will be slightly inaccurate if triggered

        if (accuracy > 0)
        {
            accuracy = 0.5f;
        }

        // Learned input for when to use laser or not
        decideWeapon(weaponChoice);


        // calculates rotation and direction to player, smoothly
        float targetUpperChestYPosition = (player != null ? (target.position.y + 1f) : target.position.y);
        Vector3 targetTransformChestPosition = new Vector3(target.position.x, targetUpperChestYPosition + accuracy, target.position.z); // adds in slight inaccuracy
        Vector3 targetDirection = targetTransformChestPosition - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(targetDirection.x, targetDirection.y, targetDirection.z));
        //lookRotation = new Quaternion(0f, lookRotation.y, lookRotation.z, lookRotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        // debug ray from barrel of sentry turret (transform.GetChild(0)) to target direction
        Debug.DrawRay(transform.GetChild(0).position, targetDirection, Color.red);

        // Attempts to shoot at guessed target
        if (playerInSight || player == null) // if player is null, we want it to keep shooting at whatever its target is, without moving to track the player
        {
            // debug ray turns green on connected sight
            Debug.DrawRay(transform.GetChild(0).position, targetDirection, Color.green);

            if (!canShoot && (player != null ? player.health : 100) < 48) // ternary operator for if target is not the player, default to value of 100
            {
                float randomValue = (float)Random.Range(0, 101);
                canShoot = randomValue > 50f;
            }

            if (isLaserCharging && canShoot)
            {
                ChargeLaser(targetDirection);
            }
            else
            {
                // Machine Gun attack
                if (Time.time >= machineGunBurstTimer && canShoot)
                {
                    ShootMachineGun(targetDirection); //new Vector3(targetX, targetY, targetZ)
                    machineGunBurstTimer = Time.time + machineGunBurstCooldown;
                }

            }
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = 0;
        actionsOut.DiscreteActions.Array[1] = 1;
        actionsOut.DiscreteActions.Array[2] = 0;

    }

    public void Update()
    {
        // setActive to false so we can respawn gameObject
        if (health <= 0)
        {
            gameObject.SetActive(false);
        }

        // the target position + 1.0f (if target = player, then the +1 aims at upper chest instead of lower on body) minus the first child position of the sentry turret which is the barrel
        Vector3 playerChestAddition = (player != null ? (new Vector3(0f, 1f, 0f)) : Vector3.zero);
        Vector3 sentryTurretTargetDirection = (target.transform.position + playerChestAddition) - transform.GetChild(0).position;
        playerInSight = IsPlayerInLineOfSight(sentryTurretTargetDirection);

        if (StepCount % GetComponent<DecisionRequester>().DecisionPeriod == 0)
        {
            //Sends observations over for training the model every frame
            RequestDecision();
        }

    }

    bool IsPlayerInLineOfSight(Vector3 direction)
    {
        // if raycast hits player in range, then can start shooting
        RaycastHit hit;
        Transform barrel = transform.GetChild(0);
        if (Physics.Raycast(barrel.position, direction, out hit, range))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    void ShootMachineGun(Vector3 targetGuess)
    {
        StartCoroutine(FireBullets(targetGuess));
        //SpawnBullet(target);
    }

    IEnumerator FireBullets(Vector3 targetGuess)
    {
        // Intermittent Rapid Fire (5 Bullets, small delay between each)
        for (int i = 0; i < 5; i++)
        {
            SpawnBullet(targetGuess);
            yield return new WaitForSeconds(machineGunBulletCooldown);
        }
    }

    void ShootLaser(Vector3 targetGuess)
    {
        StartCoroutine(BurstLaser(targetGuess));
        //SpawnBullet(target);
    }

    IEnumerator BurstLaser(Vector3 targetGuess)
    {
        // Intermittent Rapid Fire (2 lasers, small delay between each)
        for (int i = 0; i < 2; i++)
        {
            FireLaser(targetGuess);
            yield return new WaitForSeconds(laserBurstTime);
        }
    }

    void SpawnBullet(Vector3 targetGuess)
    {
        // Calculate the direction and position to the target
        Vector3 directionToTarget = targetGuess - bulletSpawnPoint.position; // Vector3 directionToTarget = target.position - bulletSpawnPoint.position; // auto-aim
        Vector3 targetPosition = bulletSpawnPoint.position + directionToTarget;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Assign the target position and tag to the bullet
        bullet.GetComponent<Bullet>().targetPosition = targetPosition;
        bullet.GetComponent<Bullet>().targetTag = "Player";

        if (bullet != null)
        {
            Destroy(bullet, 0.5f);
        }

        // Further code deals with damage and rewards for the agents

    }

    void ChargeLaser(Vector3 targetGuess)
    {
        if (laserChargeTimer < laserChargeTime)
        {
            laserChargeTimer += Time.deltaTime;
        }
        else
        {
            ShootLaser(targetGuess);

            laserChargeTimer = 0f;
            isLaserCharging = false;
        }
    }

    void FireLaser(Vector3 targetGuess)
    {
        // Calculate the direction and position to the target
        Vector3 directionToTarget = targetGuess - bulletSpawnPoint.position; // Vector3 directionToTarget = target.position - bulletSpawnPoint.position; // auto-aim
        Vector3 targetPosition = bulletSpawnPoint.position + directionToTarget;


        GameObject laser = Instantiate(laserPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Assign the target tag to the laser
        laser.GetComponent<LaserEnemy>().targetPosition = targetPosition;
        laser.GetComponent<LaserEnemy>().targetTag = "Player";

        // Restore the original rotation after firing
        // transform.rotation = originalRotation;

        if (laser != null)
        {
            Destroy(laser, 1.2f);
        }


    }


    // Agents will decide what weapon to choose depending on how it sets this variable
    void decideWeapon(bool chargeLaser)
    {
        isLaserCharging = chargeLaser;
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.CompareTag("BulletPlayer"))
    //    {
    //        //hitCounter += 1; // implemented in BulletPlayer
    //    }
    //}

}