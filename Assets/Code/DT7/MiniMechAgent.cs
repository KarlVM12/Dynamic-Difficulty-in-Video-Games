using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using EasyCharacterMovement.Templates.SideScrollerTemplate;
using TMPro;
using EasyCharacterMovement;

public class MiniMechAgent : Agent, EnemyInterface
{
    public Rigidbody rb;
    public Vector3 spawn { get; set; }

    public Transform target; // Player's Transform
    public Vector3 playerSpawn;
    public MyCharacter player;

    public float walkSpeed;
    public float runSpeed;

    public float rotationSpeed;
    private bool playerOnLeftSide = true;

    public GameObject bulletPrefab;
    public GameObject laserPrefab;

    public Transform bulletSpawnPoint;
    public Transform startingTarget;
    
    public float machineGunBulletCooldown; // 3 bullet pulse, 2 damage each
    public float machineGunBurstCooldown; // 0.2 second pause
    private float machineGunBurstTimer;
    public float range;

    public float laserChargeTime; // 3 second charge
    public float laserBurstTime; // only shoots one laser, so maybe not necessary, 10 damage
    private float laserChargeTimer;
    private bool isLaserCharging;

    public float maxHealth { get; set; } // 50 health
    public float health { get; set; }

    public int hitCounter { get; set; } = 0;
    public int playerHitCounter;

    private float awareness; // input to model that will determine how many enemies are in range of mini mech
    private float awarenessRange = 12f;

    private bool playerInSight = false;
    private bool isJumping = false;
    private float jumpResetTimer = 0f;
    public float jumpCooldownTime;
    public float jumpHeight;


    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        
        spawn = transform.position;
        maxHealth = 50;
        health = maxHealth;

        player = target.GetComponent<MyCharacter>();
        playerSpawn = player.transform.position;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((target != null ? (Vector3)target.transform.position : Vector3.zero));
        sensor.AddObservation((Vector3)transform.position);
        sensor.AddObservation(player.health);
        sensor.AddObservation(health);
        sensor.AddObservation(awareness);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float approach = actions.DiscreteActions[0]; // {0: don't approach, 1: neutral, 2: approach}
        bool weaponChoice = actions.DiscreteActions[1] > 0.5f; // {0: machine gun, 1: laser}

        float targetUpperChestYPosition = (target.position.y + 1f); //(target.GetComponent<CapsuleCollider>().height - (target.GetComponent<CapsuleCollider>().height / 3)));
        Vector3 targetTransformChestPosition = new Vector3(target.position.x, targetUpperChestYPosition, target.position.z);
        Vector3 miniMechTargetDirection = targetTransformChestPosition - transform.position;
        Vector3 barrelTargetDirection = targetTransformChestPosition - transform.GetChild(0).position;


        // Rotate the entire minimech only when the player jumps over to the other side
        if (playerOnLeftSide)
        {
            // 0f on the y-axis means we only want the rotation to affect the y-axis, so they basically spinning around
            Quaternion miniMechLookRotation = Quaternion.LookRotation(new Vector3(barrelTargetDirection.x, 0f, barrelTargetDirection.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, miniMechLookRotation, Time.deltaTime * rotationSpeed);

            // just rotates the barrel
            Quaternion lookRotation = Quaternion.LookRotation(barrelTargetDirection);
            transform.GetChild(0).rotation = Quaternion.Slerp(transform.GetChild(0).rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            // just rotates the barrel
            Quaternion lookRotation = Quaternion.LookRotation(barrelTargetDirection);
            transform.GetChild(0).rotation = Quaternion.Slerp(transform.GetChild(0).rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        // Move Decision, only if within sight of player
        if (playerInSight)
        {        
            AttemptMove(approach); 
        }

        // Weapon Decision
        decideWeapon(weaponChoice);


        Debug.DrawRay(transform.GetChild(0).position, barrelTargetDirection, Color.red);

        // Attempts to shoot at guessed target
        if (playerInSight)
        {
            Debug.DrawRay(transform.GetChild(0).position, barrelTargetDirection, Color.green);


            if (isLaserCharging)
            {
                ChargeLaser(barrelTargetDirection);
            }
            else
            {
                // Machine Gun attack
                if (Time.time >= machineGunBurstTimer)
                {
                    ShootMachineGun(barrelTargetDirection);
                    machineGunBurstTimer = Time.time + machineGunBurstCooldown;
                }
            }
        }
        else
        {
            isLaserCharging = false;
        }

    }

    // Heuristics funciton without a model, automatic max dash distance and random dash direction
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = 1; // 1: neutral{0, 1, 2}
        actionsOut.DiscreteActions.Array[1] = Random.Range(0, 2); // choose either machine gun or laser
    }

    public void Update()
    {
        // setActive to false so we can respawn gameObject
        if (health <= 0f || player.health <= 0f)
        {
            rb.velocity = Vector3.zero;
            rb.position = spawn;
            transform.position = spawn;
            gameObject.SetActive(false);
        }

        // determine if player /*chest*/ is in sight
        Vector3 miniMechTargetDirection = (player.transform.position + (new Vector3(0f, 1f, 0f))) - transform.GetChild(0).position;
        IsPlayerInLineOfSight(miniMechTargetDirection);
        //Debug.Log("Bool: " + playerInSight + " direction:" + miniMechTargetDirection);

        // less than 3 on the other side because otherwise the rotation will stop the instant the player gets over the middle of the minimech
        playerOnLeftSide = miniMechTargetDirection.x < 3;

        if (jumpResetTimer > 0)
        {
            jumpResetTimer -= Time.deltaTime;
        }
        else
        {
            isJumping = false;
        }

        // awareness functionality
        determineAwareness();
        //drawAwarenessSphere(transform.position, awarenessRange);
        
        if (StepCount % GetComponent<DecisionRequester>().DecisionPeriod == 0)
        {
            //Sends observations over for training the model every frame
            RequestDecision();
        }


    }

    void IsPlayerInLineOfSight(Vector3 direction) // changed how this function works for the minimech
    {

        // Debug.DrawRay(transform.GetChild(0).position, direction, Color.yellow);
        // if raycast hits player in range, then can start shooting
        RaycastHit hit;
        Transform barrel = transform.GetChild(0);

        if (Physics.Raycast(barrel.position, direction, out hit, range, LayerMask.GetMask("Player")))
        {
            if (hit.collider.CompareTag("Player"))
            {
                //return true;
                playerInSight = true;
                //Debug.Log("Player in Sight");
            }
            else
            {
                playerInSight = false;
                //Debug.Log(hit.collider.name + " in Sight");
            }    
        }
        else
        {
            playerInSight = false;
        }
    }

    void AttemptMove(float approach)
    {
        //=========| determining approach, speed, and distance |=========//

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        float dontApproachDistance = 6.5f;
        float neutralDistance = 4.5f;
        float approachDistance = 3f;
        float chosenDistance = neutralDistance;

        // Adjust movement speed based on approach tactic
        float moveSpeed = 0f;
        switch (approach)
        {
            case 0: // Don't approach
                if (distanceToPlayer < dontApproachDistance)
                {
                    moveSpeed = runSpeed;
                    chosenDistance = dontApproachDistance;
                }
                break;
            case 1: // Neutral
                if (distanceToPlayer < neutralDistance)
                {
                    moveSpeed = walkSpeed;
                    chosenDistance = neutralDistance;
                }
                break;
            case 2: // Approach
                if (distanceToPlayer < approachDistance)
                {
                    moveSpeed = walkSpeed;
                    chosenDistance = approachDistance;
                }
                break;
        }

        //=========| attempting movement based on approach |=========//

        Vector3 directionToPlayer = -(target.position - transform.position).normalized;
        directionToPlayer.y = 0f; // // will add only horizontal movement to mini mech

        // used for collision and jumping variable, if hasn't moved from previous position when horizontal force applied, only then should mini mech jump
        Vector3 previousMiniMechPosition = transform.position;

        // ensure that the player won't stick to walls by checking its next position
        if (CheckCollision(previousMiniMechPosition, (transform.position) + (directionToPlayer)))
        {
            // zeros out horizontal velocity, maintains vertical
            rb.velocity = Vector3.up * rb.velocity.y;

            // since its next move will result in a collision, attempts to jump over it
            if (!isJumping && distanceToPlayer < chosenDistance)
            {
                // ensures not too much horizontal velocity increases the distance and speed of jump
                Vector3 stopHorizontalVelocity = new Vector3(-1f * rb.velocity.x, jumpHeight, rb.velocity.z);
                rb.AddForce(stopHorizontalVelocity, ForceMode.Impulse);
            
                isJumping = true;
                jumpResetTimer = jumpCooldownTime;
            }
        }
        else
        {
            // if not going to collide, adds horizontal movement
            rb.AddForce(directionToPlayer * moveSpeed, ForceMode.VelocityChange);


            // if the velocity from adding force goes over intended speed, normalize it out
            if (rb.velocity.magnitude > moveSpeed)
            {
                // reduce velocity till maintains movespeed as velocity, basically undoing the previous addForce because that is what made it go over
                Vector3 horizontalNormalized = (new Vector3(rb.velocity.x, 0f, rb.velocity.z)).normalized;
                rb.AddForce(horizontalNormalized * (-moveSpeed), ForceMode.VelocityChange);

            }

            //Debug.Log("Force: " + (directionToPlayer * moveSpeed * Time.deltaTime) + " V: " + rb.velocity);


            // if tried to move and still within distance to player, will attempt to jump to move
            if (!isJumping && distanceToPlayer < chosenDistance && jumpResetTimer <= 0)
            {

                // will only alow mech to jump when its been stuck in same position (x position difference is within range [-0.1, 0.1]) after attempting a move because of collision, i.e. wall/jump up ledge,as to get away from player
                //float positionDifference = (previousMiniMechPosition.x - transform.position.x);
                //if (positionDifference >= -0.1 && positionDifference <= 0.1)
                //{

                // ensures not too much horizontal velocity increases the distance and speed of jump
                Vector3 stopHorizontalVelocity = new Vector3(-1f * rb.velocity.x, jumpHeight, rb.velocity.z);
                rb.AddForce(stopHorizontalVelocity, ForceMode.Impulse);
                
                //}

                isJumping = true;
                jumpResetTimer = jumpCooldownTime;
            }
        }

    }

    private bool CheckCollision(Vector3 start, Vector3 end)
    {
        // no mask
        RaycastHit hit;

        // with mask
        //int layerMask = ~LayerMask.GetMask("Default");  // Ignore collisions with the "Default" layer
        //Physics.Raycast(start, end - start, out hit, Vector2.Distance(start, end), layerMask);

        Debug.DrawRay(start, end - start, Color.green, 1.1f);

        if (Physics.Raycast(start, end - start, out hit, Vector2.Distance(start, end)))
        {
            return true;
        }

        return false;
    }

    void determineAwareness()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, awarenessRange);
            
        // Count the number of enemies within the awareness range
        int enemyCount = 0;
        foreach (Collider collider in colliders)
        {
            // check if an enemy as well as see if it is the parent enemy object since only the parent is of type Agent
            if (collider.CompareTag("Enemy") && collider.gameObject.GetComponent<Agent>() != null && collider.gameObject != gameObject)
            {
                enemyCount++;
                //Debug.Log("Awareness Detected: " + collider.name + " Level: " + enemyCount);
            }
        }

        // Update awareness level based on the number of enemies detected
        if (enemyCount >= 2)
        {
            awareness = 2f; // High awareness
        }
        else if (enemyCount >= 1)
        {
            awareness = 1f; // moderate awareness
        }
        else
        {
            awareness = 0f; // Low awareness
        }
    }

    // draw sphere for debugging
    void drawAwarenessSphere(Vector3 center, float radius)
    {
        int numRays = 36; // Number of rays to cast
        float angleIncrement = 360f / numRays;

        for (int i = 0; i < numRays; i++)
        {
            float angle = i * angleIncrement;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            RaycastHit hit;

            if (Physics.Raycast(center, direction, out hit, radius))
            {
                Debug.DrawLine(center, hit.point, Color.red);
            }
            else
            {
                Vector3 end = center + direction * radius;
                Debug.DrawLine(center, end, Color.green);
            }
        }
    }

    void ShootMachineGun(Vector3 targetGuess)
    {
        StartCoroutine(FireBullets(targetGuess));
        //SpawnBullet(target);
    }

    IEnumerator FireBullets(Vector3 targetGuess)
    {
        // Intermittent Rapid Fire (3 Bullets, small delay between each)
        for (int i = 0; i < 3; i++)
        {
            SpawnBullet(targetGuess);
            yield return new WaitForSeconds(machineGunBulletCooldown);
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

    }


    void ChargeLaser(Vector3 targetGuess)
    {
        if (laserChargeTimer < laserChargeTime)
        {
            laserChargeTimer += Time.deltaTime;
        }
        else
        {
            FireLaser(targetGuess); // only shoots one laser so don't need a coroutine/IEnumerator

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
        laser.GetComponent<LaserEnemy>().damage = 10.0f;

        // Restore the original rotation after firing
        //transform.rotation = originalRotation;

        if (laser != null)
        {
            Destroy(laser, 1.2f);
        }


    }

    //Agents will decide what weapon to choose depending on how it sets this variable
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
