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

    public Transform target; // Player's Transform
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

    private Vector3 lastKnownPlayerPosition;
    public float health { get; set; }
    public float maxHealth { get; set; }
    //private float playerHealth = 100;
    public Vector3 spawn {  get; set; }

    public int hitCounter { get; set; }
    public int playerHitCounter = 0;

    public Transform startingTarget;

    private void Awake()
    {
        player = target.GetComponent<MyCharacter>();
        playerSpawn = player.transform.position;
        lastKnownPlayerPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        player = target.GetComponent<MyCharacter>();

        // Print end of episode
        Debug.Log("====END EPISODE====");
        Debug.Log("Player Health: " + player.health);
        Debug.Log("Player Hit Counter: " + player.hitCounter);
        Debug.Log("Turret Health: " + health);
        Debug.Log("Turret Hit Counter: " + hitCounter);

        // Randomize player health
        float randomValue = (float)Random.Range(1, 101);
        player.health = randomValue;

        // reset turret health
        health = 45;

        // reset hit counters
        hitCounter = 0;
        playerHitCounter = 0;
        player.hitCounter = 0;

        // move player transform to beginning
        player.transform.position = playerSpawn;

        // Reset last known position
        //lastKnownPlayerPosition = startingTarget.position;

        // Print new episode
        Debug.Log("====NEW EPISODE====");
        Debug.Log("Player Health: " + player.health);
        Debug.Log("Player Hit Counter: " + player.hitCounter);
        Debug.Log("Turret Health: " + health);
        Debug.Log("Turret Hit Counter: " + hitCounter);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector3)target.transform.position);
        sensor.AddObservation((Vector3)transform.position);
        sensor.AddObservation(player.health);
        sensor.AddObservation(health);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        bool weaponChoice = actions.DiscreteActions[0] > 0.5f; // charges laser if over threshold
        bool canShoot = actions.DiscreteActions[1] > 0.5f; // can shoot if above threshold
        float accuracy = actions.DiscreteActions[2]; // will be slightly inaccurate if triggered
        //float targetX = actions.ContinuousActions[0];
        //float targetY = actions.ContinuousActions[1];
        //float targetZ = actions.ContinuousActions[2]; // not really necessary
        
        if (accuracy > 0)
        {
            accuracy = 0.5f;
        }

        // Learned input for when to use laser or not
        decideWeapon(weaponChoice);

        // Turret looks at target pos
        //Vector3 targetTransformMiddle = new Vector3(target.position.x, (target.position.y + (target.GetComponent<CapsuleCollider>().height - (target.GetComponent<CapsuleCollider>().height / 3))), target.position.z);
        //Vector3 targetDirection = new Vector3(targetX, targetY, targetZ);//target.position - transform.position; // if we were training continuous for shooting pos


        float targetUpperChestYPosition = (target.position.y + 1f);//(target.GetComponent<CapsuleCollider>().height - (target.GetComponent<CapsuleCollider>().height / 3)));
        Vector3 targetTransformChestPosition = new Vector3(target.position.x, targetUpperChestYPosition + accuracy, target.position.z); // adds in slight inaccuracy
        Vector3 targetDirection = targetTransformChestPosition - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);


        //targetDirection = IsPlayerInLineOfSight(targetDirection) ? targetDirection : (lastKnownPlayerPosition - transform.position);

        // Attempts to shoot at guessed target
        if (IsPlayerInLineOfSight(targetDirection))
        {
        // in case turret has rotated away from target, for a smooth rotation towards target so turret doesn't just snap to target
        //Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);


        Debug.DrawRay(transform.position, targetDirection, Color.red);


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

        
        // ===! NEED TO END EPISODE AND FIND OUT IF PLAYER GOT HIT !=== //
        // Assigns rewards
        if (player.hitCounter != 0)
        {
            AddReward(4.0f * player.hitCounter);
            player.hitCounter = 0;

            // Specific weapon in health range
            if (isLaserCharging && player.health > 80)
            {
                AddReward(1.5f);
            }
        }
        else
        {
            // missed player
            AddReward(-0.2f);

            // Miss when player has < 25%
            if (player.health < 20)
            {
                AddReward(0.5f);
            }
        }

        // Hit Laser when turretHealth < 32%
        if (isLaserCharging && health < 8)
        {
            AddReward(1f);
        }

        // Turret health <= 0
        if (health <= 0)
        {
            AddReward(-0.1f);
            EndEpisode();
        }

        if (player.health <= 0)
        {
            AddReward(1f);
            //Debug.Log("PLAYER DIED");
            EndEpisode();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // can be used like update to make decisions via the agent to auto aim at player location
        //float targetUpperChestYPosition = (target.position.y + (target.GetComponent<CapsuleCollider>().height - (target.GetComponent<CapsuleCollider>().height / 4)));
        //Vector3 targetTransformChestPosition = new Vector3(target.position.x, targetUpperChestYPosition, target.position.z);
        //Vector3 targetDirection = targetTransformChestPosition - transform.position;
        //Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        //actionsOut.DiscreteActions.Array[0] = 0;
        //actionsOut.ContinuousActions.Array[0] = targetDirection.x;
        //actionsOut.ContinuousActions.Array[1] = targetDirection.y;
        //actionsOut.ContinuousActions.Array[2] = targetDirection.z;

        actionsOut.DiscreteActions.Array[0] = 0;
        actionsOut.DiscreteActions.Array[1] = 1;
        actionsOut.DiscreteActions.Array[2] = 0;

    }

    public void Update()
    {
        if (StepCount % GetComponent<DecisionRequester>().DecisionPeriod == 0)
        {
            //Sends observations over for training the model every frame
            RequestDecision();
        }

        // If we want to use the Sentry Turret as auto-aim, not as an agent
        //Vector3 targetDirection = target.position - transform.position;

        //if (IsPlayerInLineOfSight(targetDirection))
        //{
        //    // in case turret has rotated away from target, for a smooth rotation towards target so turret doesn't just snap to target
        //    Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        //    Debug.DrawRay(transform.position, targetDirection, Color.red);


        //    if (isLaserCharging)
        //    {
        //        ChargeLaser();
        //    }
        //    else
        //    {
        //        // Machine Gun attack
        //        if (Time.time >= machineGunBurstTimer)
        //        {
        //            ShootMachineGun();
        //            machineGunBurstTimer = Time.time + machineGunBurstCooldown;
        //        }

        //    }
        //}
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
                //float targetUpperChestYPosition = (target.position.y + (target.GetComponent<CapsuleCollider>().height - (target.GetComponent<CapsuleCollider>().height / 4)));
                //Vector3 targetTransformChestPosition = new Vector3(target.position.x, targetUpperChestYPosition, target.position.z);
                //lastKnownPlayerPosition = targetTransformChestPosition;
                return true;
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
        // Need to still implement the laser beam, just printing a message for now
        // Debug.Log("Laser Beam Fired - Deal " + laserDamage + " damage!!!!");

        // Calculate the direction and position to the target
        Vector3 directionToTarget = targetGuess - bulletSpawnPoint.position; // Vector3 directionToTarget = target.position - bulletSpawnPoint.position; // auto-aim
        Vector3 targetPosition = bulletSpawnPoint.position + directionToTarget;


        GameObject laser = Instantiate(laserPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Assign the target tag to the laser
        laser.GetComponent<LaserEnemy>().targetPosition = targetPosition;
        laser.GetComponent<LaserEnemy>().targetTag = "Player";

        // Restore the original rotation after firing
        //transform.rotation = originalRotation;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("BulletPlayer"))
        {
            //hitCounter += 1; // implemented in BulletPlayer
        }
    }

}
