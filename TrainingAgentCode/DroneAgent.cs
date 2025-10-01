using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using EasyCharacterMovement.Templates.SideScrollerTemplate;
using Unity.VisualScripting;

public class DroneAgent : Agent, EnemyInterface
{
    private Rigidbody droneRigidbody;
    public Vector3 spawn { get; set; }

    public Transform target; // Player's Transform
    public Vector3 playerSpawn;
    public MyCharacter player;

    public float moveSpeed = 6f;
    private float maxLeft;
    private float maxRight;


    public float rotationSpeed = 5f;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public Transform startingTarget;
    public float machineGunBulletCooldown = 0.15f;
    public float machineGunBurstCooldown = 1.5f;
    private float machineGunBurstTimer;
    public float range;


    public float maxHealth { get; set; }
    public float health { get; set; }


    public int hitCounter { get; set; }
    public int playerHitCounter = 0;

    private float yHeight;

    private bool isDashing = false;
    public float dashDuration;
    public float dashCooldown;
    private float dashCooldownTimer = 0f;

    private void Awake()
    {
        droneRigidbody = GetComponent<Rigidbody>();

        player = target.GetComponent<MyCharacter>();
        playerSpawn = player.transform.position;

        maxLeft = transform.position.x - 6;
        maxRight = transform.position.x + 6;

        droneRigidbody.velocity = Vector3.left * moveSpeed;

        yHeight = transform.position.y;
    }

    public override void OnEpisodeBegin()
    {
        player = target.GetComponent<MyCharacter>();

        // Print end of episode
        Debug.Log("====END EPISODE====");
        Debug.Log("Player Health: " + player.health);
        Debug.Log("Player Hit Counter: " + player.hitCounter);
        Debug.Log("Drone Health: " + health);
        Debug.Log("Drone Hit Counter: " + hitCounter);

        // Randomize player health
        float randomValue = (float)Random.Range(1, 101);
        player.health = randomValue;

        // reset turret health
        health = maxHealth;

        // reset hit counters
        hitCounter = 0;
        playerHitCounter = 0;
        player.hitCounter = 0;

        // move player transform to beginning
        player.transform.position = playerSpawn;

        // Print new episode
        Debug.Log("====NEW EPISODE====");
        Debug.Log("Player Health: " + player.health);
        Debug.Log("Player Hit Counter: " + player.hitCounter);
        Debug.Log("Drone Health: " + health);
        Debug.Log("Drone Hit Counter: " + hitCounter);
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
        float dashDistance = actions.DiscreteActions[0] + 1.0f; // add one because NN starts from 0 so + 1 --> {1, 2, 3, 4}
        float dashDirection = actions.DiscreteActions[1]; // 0 for left, 1 for right

        if (dashDirection <= 0.5f)
        {
            dashDirection = -1.0f;
        }

        float targetUpperChestYPosition = (target.position.y + 1f); //(target.GetComponent<CapsuleCollider>().height - (target.GetComponent<CapsuleCollider>().height / 3)));
        Vector3 targetTransformChestPosition = new Vector3(target.position.x, targetUpperChestYPosition, target.position.z); 
        Vector3 targetDirection = targetTransformChestPosition - transform.position;
        
        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        if (hitCounter != 0)
        {
            StartDash(dashDistance, dashDirection);
            hitCounter = 0;
        }

        // Attempts to shoot at guessed target
        if (IsPlayerInLineOfSight(targetDirection))
        {
             Debug.DrawRay(transform.position, targetDirection, Color.red);

            // Machine Gun attack
            if (Time.time >= machineGunBurstTimer)
            {
                ShootMachineGun(targetDirection); 
                machineGunBurstTimer = Time.time + machineGunBurstCooldown;
            }

        }
        else
        {
            Move();
        }


        // Assigns rewards
        if (player.hitCounter != 0)
        {
            AddReward(4.0f * player.hitCounter);
            player.hitCounter = 0;
        }
        else
        {
            // missed player
            AddReward(-0.2f);

            // Miss when player has < 20%
            //if (player.health < 20)
            //{
            //    AddReward(0.5f);
            //}
        }

        if (player.health >= 80f)
        {
            if (dashDistance >= 3.5f)
            {
                AddReward(0.5f);
            }
            else
            {
                AddReward(-0.1f);
            }
        }
        else if (player.health > 30f && player.health < 80f)
        {
            if (dashDistance >= 1.1f && dashDistance < 3.5f)
            {
                AddReward(0.5f);
            }
            else
            {
                AddReward(-0.1f);
            }
        }
        else if (player.health <= 20f)
        {
            if (dashDistance < 1.1f)
            {
                AddReward(0.5f);
            }
            else
            {
                AddReward(-0.1f);
            }
        }

        // Turret health <= 0
        if (health <= 0)
        {
            AddReward(-0.2f);
            EndEpisode();
        }

        if (player.health <= 0)
        {
            AddReward(1f);
            //Debug.Log("PLAYER DIED");
            EndEpisode();
        }

    }

    // Heuristics funciton without a model, automatic max dash distance and random dash direction
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = 3; // max dash distance {0, 1, 2, 3} --> {1, 2, 3, 4}
        actionsOut.DiscreteActions.Array[1] = Random.Range(0, 2); // random left or right dash
    }

    public void Update()
    {
        // keeps drone from being knocked off its y location
        transform.position = new Vector3(transform.position.x, yHeight, transform.position.z); 

        if (StepCount % GetComponent<DecisionRequester>().DecisionPeriod == 0)
        {
            //Sends observations over for training the model every frame
            RequestDecision();
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
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

    void Move()
    {
        //Debug.Log("Move Range: (" + maxLeft + ", " + maxRight + ") Current Pos: " + );
        if (transform.position.x >= maxRight)
        {
            droneRigidbody.velocity = Vector3.left * moveSpeed;
        }
        else if (transform.position.x <= maxLeft)
        {
            droneRigidbody.velocity = Vector3.right * moveSpeed;
        }
    }

    //void Dash(float distance, float direction)
    //{
    //    //Vector3 distanceVector = new Vector3(distance * direction, 0, 0);
    //    //droneRigidbody.velocity = distanceVector * moveSpeed;
        
    //}

    void StartDash(float distance, float direction)
    {
        if (!isDashing && dashCooldownTimer <= 0)
        {
            StartCoroutine(Dash(distance, direction));
        }
    }

    IEnumerator Dash(float distance, float direction)
    {
        isDashing = true;
        float elapsedTime = 0f;

        // distance & dash able to dash
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x + (direction >= 0f ? distance : -distance), startPos.y, startPos.z);

        // dashes for distance within duration, but if it collides with something it stops
        bool collided = false;
        while (elapsedTime < dashDuration)
        {
            // without accounting for collision
            //transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / dashDuration);

            // dash animation in code by interpolating frames inbetween remaining end position and duration left on dash
            float t = elapsedTime / dashDuration;
            Vector3 newPosition = Vector3.Lerp(startPos, endPos, t);

            // Check for collisions along the movement path
            if (CheckCollision(startPos, newPosition))
            {
                collided = true;
                //transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
                break;
            }

            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure we end up exactly at the destination
        if (!collided)
        {
            transform.position = new Vector3(endPos.x, endPos.y, transform.position.z); // Ensure we end up exactly at the destination
        }

        isDashing = false;
        dashCooldownTimer = dashCooldown;
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
            //Debug.Log("Collision detected: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject.name.StartsWith("Drone") || hit.collider.gameObject.name.Equals("Barrel")) // if it detects a collision with itself
            {
                return false;
            }
            
            return true;
        }

        return false;
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.CompareTag("BulletPlayer"))
    //    {
    //        //hitCounter += 1; // implemented in BulletPlayer
    //    }
    //}

}
