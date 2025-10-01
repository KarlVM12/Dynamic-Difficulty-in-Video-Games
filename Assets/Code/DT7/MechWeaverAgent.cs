using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using EasyCharacterMovement.Templates.SideScrollerTemplate;

public class MechWeaverAgent : Agent, EnemyInterface
{
    public Rigidbody rb; 
    public Vector3 spawn { get; set; }

    public Transform target; // Player's Transform
    public Vector3 playerSpawn;
    public MyCharacter player;

    public GameObject bulletPrefab;

    public ProgressBar HealthBar;
    public float totalHealth;

    public float maxHealth { get; set; }
    public float health { get; set; }

    public float range;
    private float moveSpeed;

    public int hitCounter { get; set; } = 0;
    public int playerHitCounter;

    private bool playerInSight = false;

    private List<MechWeaverLeg> legArray = new List<MechWeaverLeg>();
    private List<MechWeaverLeg> attackingArray = new List<MechWeaverLeg>();

    private List<Transform> bulletSpawnPoints = new List<Transform>();
    private List<Transform> bulletSpawnPointsSecondary = new List<Transform>();

    private bool isAttacking = false;
    private float isAttackingCooldown = 1.5f;
    private float isAttackingTimer = 0.0f;
    private bool ableToBeDamaged; // true only in stage 4, for main body

    private float bulletSpreadCooldown = 2.5f;
    private float bulletSpreadTimer = 0.0f;
    private float bulletSpreadPulse = 1;

    private bool noRotateLimits;
    private Quaternion initialRotation;
    private bool rotateToZero = false;

    private bool atYMinimum;
    private float yMinimum;
    private float yMaximum;
    private float xMinimum;
    private float xMaximum;

    public int currentPhase;
    private int legCount;
    private float difficultyModifier; // used for increasing difficulty in each stage, along with being modified by AI

    private int maxDrones;
    public int currentDrones;
    [SerializeField] GameObject mechWeaverDronePrefab;
    [SerializeField] Transform droneSpawnPoint;
    private float droneSpawnCooldown = 2.5f;
    private float droneSpawnTimer = 0.0f;

    [SerializeField] GameObject mechWeaverString;

    public bool isResetting;
    private bool phase1Reset;
    private bool phase2Reset;
    private bool phase3Reset;
    private bool phase4Reset;
    private float resetCooldown = 2f; // after resetPosition, won't move for 2 seconds
    private float resetTimer = 0.0f;

    public bool playerEnteredArena = false;

    public void Start()
    {
        maxHealth = 200f;
        health = maxHealth;


        rb = transform.GetComponent<Rigidbody>();

        spawn = transform.position;

        moveSpeed = 10f;

        player = target.GetComponent<MyCharacter>();
        playerSpawn = player.transform.position;

        for (int i = 0; i < 8; i++)
        {
            legArray.Add(transform.GetChild(i).GetComponent<MechWeaverLeg>());
            //Debug.Log(transform.GetChild(i).name);
        }

        // gets the scene list of bulletSpawnPoint's parents
        Transform bulletSpawnPointsParent = transform.GetChild(8);
        for (int i = 0; i < 12; i++)
        {
            bulletSpawnPoints.Add(bulletSpawnPointsParent.GetChild(i).GetComponent<Transform>());
        }

        // gets the scene list of seconadryBulletSpawnPoint's parents
        Transform bulletSpawnPointsSecondaryParent = transform.GetChild(9);
        for (int i = 0; i < 12; i++)
        {
            bulletSpawnPointsSecondary.Add(bulletSpawnPointsSecondaryParent.GetChild(i).GetComponent<Transform>());
        }

        ableToBeDamaged = false;

        noRotateLimits = false;
        initialRotation = transform.rotation;

        atYMinimum = false; // for if it hits the bottom of its arena range so its parts don't squash
        yMinimum = 3f;
        yMaximum = 6.0f;
        xMinimum = -8.75f;
        xMaximum = 7.25f;

        currentPhase = 1; // 1: leg attack, 2: start bullets, 3: spawn drones, 4: add another bullet spread
        legCount = legArray.Count;
        difficultyModifier = 1f;

        maxDrones = 1;
        currentDrones = 0;

        isResetting = true;
        phase1Reset = false;
        phase2Reset = false;
        phase3Reset = false;
        phase4Reset = false;

        totalHealth = maxHealth + (legArray[0].maxHealth * 8);
        Debug.Log("Total: " + totalHealth);

        if (HealthBar != null && HealthBar.gameObject.activeSelf)
        {
            HealthBar.BarValue = 100f;
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation((Vector3)target.transform.position);
        //sensor.AddObservation((Vector3)transform.position);
        //sensor.AddObservation(player.health);
        //sensor.AddObservation(health);
        //sensor.AddObservation(awareness);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //float approach = actions.DiscreteActions[0]; // {0: don't approach, 1: neutral, 2: approach}
        //bool weaponChoice = actions.DiscreteActions[1] > 0.5f; // {0: machine gun, 1: laser}

        //float targetUpperChestYPosition = (target.position.y + 1f); //(target.GetComponent<CapsuleCollider>().height - (target.GetComponent<CapsuleCollider>().height / 3)));
        //Vector3 targetTransformChestPosition = new Vector3(target.position.x, targetUpperChestYPosition, target.position.z);
        //Vector3 miniMechTargetDirection = targetTransformChestPosition - transform.position;
        //Vector3 barrelTargetDirection = targetTransformChestPosition - transform.GetChild(0).position;


        //// Rotate the entire minimech only when the player jumps over to the other side
        //if (playerOnLeftSide)
        //{
        //    // 0f on the y-axis means we only want the rotation to affect the y-axis, so they basically spinning around
        //    Quaternion miniMechLookRotation = Quaternion.LookRotation(new Vector3(barrelTargetDirection.x, 0f, barrelTargetDirection.z));
        //    transform.rotation = Quaternion.Slerp(transform.rotation, miniMechLookRotation, Time.deltaTime * rotationSpeed);

        //    // just rotates the barrel
        //    Quaternion lookRotation = Quaternion.LookRotation(barrelTargetDirection);
        //    transform.GetChild(0).rotation = Quaternion.Slerp(transform.GetChild(0).rotation, lookRotation, Time.deltaTime * rotationSpeed);
        //}
        //else
        //{
        //    // just rotates the barrel
        //    Quaternion lookRotation = Quaternion.LookRotation(barrelTargetDirection);
        //    transform.GetChild(0).rotation = Quaternion.Slerp(transform.GetChild(0).rotation, lookRotation, Time.deltaTime * rotationSpeed);
        //}

        //// Move Decision, only if within sight of player
        //if (playerInSight)
        //{
        //    AttemptMove(approach);
        //}

        //// Weapon Decision
        //decideWeapon(weaponChoice);


        //Debug.DrawRay(transform.GetChild(0).position, barrelTargetDirection, Color.red);

        //// Attempts to shoot at guessed target
        //if (playerInSight)
        //{
        //    Debug.DrawRay(transform.GetChild(0).position, barrelTargetDirection, Color.green);


        //    if (isLaserCharging)
        //    {
        //        ChargeLaser(barrelTargetDirection);
        //    }
        //    else
        //    {
        //        // Machine Gun attack
        //        if (Time.time >= machineGunBurstTimer)
        //        {
        //            ShootMachineGun(barrelTargetDirection);
        //            machineGunBurstTimer = Time.time + machineGunBurstCooldown;
        //        }
        //    }
        //}
        //else
        //{
        //    isLaserCharging = false;
        //}


        ////======| Assigns rewards |======// 

        //if (player.hitCounter != 0)
        //{
        //    AddReward(2.0f * player.hitCounter);
        //    player.hitCounter = 0;

        //    if (weaponChoice) // laser hit
        //    {
        //        AddReward(0.5f);

        //        if (approach < 1.0f) // don't approach (0)
        //        {
        //            AddReward(0.5f);
        //        }
        //    }
        //}
        //else
        //{
        //    // missed player
        //    AddReward(-0.2f);

        //}

        //// awareness rewards
        //if (awareness > 1.5f) // awareness >= 2
        //{
        //    if (approach > 1.5f) // approach (2)
        //    {
        //        AddReward(-0.5f);
        //    }
        //    else if (approach > 0.5f) // neutral approach (1)
        //    {
        //        AddReward(-0.1f);
        //    }
        //    else // don't approach (0)
        //    {
        //        AddReward(0.2f);
        //    }
        //}
        //else if (awareness > 0.5f && awareness < 1.5f) // awareness = 1
        //{
        //    if (approach > 1.5f) // approach (2)
        //    {
        //        AddReward(-0.2f);
        //    }
        //}


        //if (health > 0 && health <= (maxHealth * .2)) // turret at 20% health
        //{
        //    if (approach < 0.5f) // dont approach (0)
        //    {
        //        AddReward(1.0f);
        //    }
        //}
        //else if (health <= 0)
        //{
        //    AddReward(-0.2f);
        //    EndEpisode();
        //}

        //if (player.health >= 80f)
        //{
        //    if (approach > 1.5f && approach < 2.5f) // approach (2)
        //    {
        //        AddReward(1.0f);
        //    }
        //}
        //else if (player.health > 30f && player.health < 80f)
        //{
        //    if (approach > 0.5f && approach < 1.5f) // neutral (1)
        //    {
        //        AddReward(0.5f);
        //    }
        //}
        //else if (player.health <= 20f)
        //{
        //    if (approach > 0.5f && approach < 1.5f) // neutral (1)
        //    {
        //        AddReward(0.5f);
        //    }

        //    if (weaponChoice)
        //    {
        //        AddReward(-0.5f); // don't use laser when player's health is low
        //    }

        //    // need to add negative reward for using laser below 20%
        //}

        //if (player.health <= 0)
        //{
        //    AddReward(1f);
        //    //Debug.Log("PLAYER DIED");
        //    EndEpisode();
        //}

    }

    // Heuristics funciton without a model, automatic max dash distance and random dash direction
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //actionsOut.DiscreteActions.Array[0] = 1; // 1: neutral{0, 1, 2}
        //actionsOut.DiscreteActions.Array[1] = Random.Range(0, 2); // choose either machine gun or laser
    }

    public void FixedUpdate()
    {

        //==========| Movement Logic |==========// 
        // Will move on a string attached to the ceiling and follow the player around the arena

        // can only move if player exists, is not attacking, not resetting, and not on the last stage
        if (player != null && !isAttacking && !isResetting && currentPhase != 4)
        {
            if ((rb.position.x > xMinimum && rb.position.x < xMaximum))
            {
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

                Vector3 move = directionToPlayer * moveSpeed * Time.deltaTime;

                // if under minimum, will slightly keep the rigidbody above so that it would get stuck at the minimum value
                if (rb.position.y < yMinimum)
                {
                    if (move.y < 0f)
                    {
                        move = new Vector3(move.x, 0.001f, move.z);
                    }
                }
                else if (rb.position.y > yMaximum)
                {
                    if (move.y > 0f)
                    {
                        move = new Vector3(move.x, -0.001f, move.z);
                    }
                }

                if (!rotateToZero)
                {
                    rotateToZero = true;
                    transform.rotation = new Quaternion(0, 0, 0, 1);
                }
                rb.MovePosition(rb.position + move);

                //if (atYMinimum)
                //{
                //    move = new Vector3(directionToPlayer.x, 2f, directionToPlayer.z) * moveSpeed * Time.deltaTime;
                //    rb.MovePosition(rb.position + move);
                //}

                //if (atYMinimum && rb.position.y > yMinimum + 1.25f)
                //{
                //    atYMinimum = false;
                //}
                
            }
            else
            {
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

                Vector3 move = directionToPlayer * moveSpeed * Time.deltaTime;

                if (rb.position.x < xMinimum)
                {
                    if (move.x < 0f)
                    {
                        move = new Vector3(0.001f, move.y, move.z);
                    }
                }
                else if (rb.position.x > xMaximum)
                {
                    if (move.x > 0f)
                    {
                        move = new Vector3(-0.001f, move.y, move.z);
                    }
                }

                // if under minimum, will slightly keep the rigidbody above so that it would get stuck at the minimum value
                if (rb.position.y < yMinimum)
                {
                    if (move.y < 0f)
                    {
                        move = new Vector3(move.x, 0.001f, move.z);
                        atYMinimum = true;
                    }
                }
                else if (rb.position.y > yMaximum)
                {
                    if (move.y > 0f)
                    {
                        move = new Vector3(move.x, -0.001f, move.z);
                    }
                }

                if (!rotateToZero)
                {
                    rotateToZero = true;
                    transform.rotation = new Quaternion(0, 0, 0, 1);
                }
                rb.MovePosition(rb.position + move);

            }
        }
    }

    public void Update()
    {
        //if (StepCount % GetComponent<DecisionRequester>().DecisionPeriod == 0)
        //{
        //    //Sends observations over for training the model every frame
        //    RequestDecision();
        //}

        if (HealthBar != null && HealthBar.gameObject.activeSelf)
        {
            HealthBar.BarValue = (int)((currentTotalHealth() / totalHealth) * 100); // dividing by total health limits range to 0-100 instead of 0-600
        }

        if (ableToBeDamaged && health <= 0f)
        {
            ableToBeDamaged = false;
            
            // enable gravity, disable kinematic, fall to ground, disappear
            rb.useGravity = true;
            rb.isKinematic = false;
            Destroy(mechWeaverString);
            StartCoroutine(destroyMechWeaver());
            //Destroy(gameObject, 1f);
        }


        // Keeps the string on same x position so it doesn't jump around when MechWeaver rotates
        if (mechWeaverString != null)
        {
            mechWeaverString.transform.position = new Vector3(transform.position.x, transform.position.y+4, mechWeaverString.transform.position.z);
        }

        // Recount Legs to determine phase
        RecountLegs();

        if (legCount <= 0 && health > 0f) // phase 4
        {
            currentPhase = 4;
            
            // lock position to middle of arena
            resetPosition();
            isResetting = false; // will allow an attack this frame, but not movement
             
            moveSpeed = 18f;
            maxDrones = 3;
            
        }
        else if (legCount <= 2) // phase 3
        {
            
            currentPhase = 3;
            
            // zoom to middle quickly
            if (!phase3Reset)
            {
                resetPosition();
                phase3Reset = true;
            }
            else
            {
                moveSpeed = 15f;
                maxDrones = 2;
            }
        }
        else if (legCount <= 4) // phase 2
        {
            currentPhase = 2;
        
            // zoom to middle quickly
            if (!phase2Reset)
            { 
                resetPosition();
                phase2Reset = true;
            }
            else
            {
                moveSpeed = 12f;
            }

        }
        else // phase 1 starting sequence, wait five seconds, load up MechWeaver Health bar, and start
        {
            if (!phase1Reset && playerEnteredArena)
            {
                StartCoroutine(phase1StartTime());
                phase1Reset = true;
                // need to activate health bar

            }
        }


        // phase statements for bullet spiral, transforms and directions, and spawn mini spiders
        if (!isResetting && health > 0f)
        {
            if (currentPhase >= 4) // might need to move this out, because even tho im setting isResetting to true right after the attack, there is still mvmt jitter
            {

                // increase bullet pulse, make main body able to be hit
                bulletSpreadAttack(); // will shoot seconadry bullet spread at phase four
                spawnDrones();
                ableToBeDamaged = true;

            }
            else if (currentPhase >= 3)
            {
                // Start to spawn little drones at stage 3
                bulletSpreadAttack();
                spawnDrones();
                health = maxHealth;

            }
            else if (currentPhase >= 2)
            {
                bulletSpreadAttack();
                health = maxHealth;
                noRotateLimits = true;
            }
            else // Phase 1
            {
                health = maxHealth;
            }
        }

        Vector3 direcitonToPlayer = player.transform.position + (new Vector3(0.0f, 1f, 0.0f)) - transform.position; // y + 1f gets accurate chest position of player
        Debug.DrawRay(transform.position, direcitonToPlayer.normalized * 10, Color.yellow);
        

        //==========| Leg Attacking Logic |==========// 
        // Being the body of the legs, it will allow one leg to attack at a time, but only up to when that first leg attacks, another leg can attack immediately after
        // Will have to add stages and scaling to all the timers and values to speed up/slow down

        if (isAttackingTimer > (isAttackingCooldown / 2)) // decreases timer up to half of cooldown time
        {
            isAttackingTimer -= Time.deltaTime;
        }
        else if (isAttackingTimer > 0.0f) // once past half of cooldown time, only then will rotate back, gives an effect that the attack was heavy
        {
            isAttackingTimer -= Time.deltaTime;
            if (!rotateToZero)
            {
                StartCoroutine(rotateBackToZero());
            }
        }
        else
        {
            isAttacking = false;
            rotateToZero = false;
        }

        if (!isAttacking && !isResetting && legCount > 0 && health > 0f)
        {
            attackingArray.Clear();

            // see which legs are able to attack
            foreach (MechWeaverLeg leg in legArray)
            {
                if (leg.isAttacking)
                {
                    attackingArray.Add(leg);
                }
                //else
                //{
                //    attackingArray.Remove(leg);
                //}
            }

            // Of the legs able to attack, get the closest one
            MechWeaverLeg closestLeg = null;
            float minDistanceToPlayer = Mathf.Infinity;

            foreach (MechWeaverLeg leg in attackingArray)
            {
                //Debug.Log("Leg Distance: " + leg.LegArm.parent.parent.name + " D:" + distanceToPlayer );

                if (leg.distanceToPlayer < minDistanceToPlayer)
                {
                    minDistanceToPlayer = leg.distanceToPlayer;
                    closestLeg = leg;
                }
            }

            //Debug.Log("Closest Leg: " + closestLeg.LegArm.parent.parent.name);
            if (closestLeg != null)
            {
                isAttacking = true;
                isAttackingTimer = isAttackingCooldown;

                // attempting to rotate towards player when attacking using the angle of the VisionPoint at the end of LegArm, instead of LegForearm since LegArm is the angle the Body will be aiming at player with
                Vector3 legDirecitonToPlayer = player.transform.position + (new Vector3(0.0f, 1f, 0.0f)) - closestLeg.LegArmVisionPoint.position;
                //Debug.DrawRay(closestLeg.LegArmVisionPoint.position, legDirecitonToPlayer.normalized * 10, Color.yellow, 2f);
                
                // rotation of both main body and current legVision
                Quaternion targetRotation = Quaternion.LookRotation(direcitonToPlayer);
                Quaternion legTargetRotation = Quaternion.LookRotation(legDirecitonToPlayer);

                // only z rotations
                targetRotation.x = 0;
                targetRotation.y = 0;
                legTargetRotation.x = 0;
                legTargetRotation.y = 0;

                // readable rotation units
                Vector3 euler = targetRotation.eulerAngles;
                Vector3 legEuler = legTargetRotation.eulerAngles;
                Debug.Log("Leg Rotate: " + euler.z + " " + legEuler.z);

                // since the legs are all on their own angles and simply moving the main body would overshoot the fixed angles of the legs, have to calculate the difference
                float desiredZRotation = legEuler.z - euler.z;
                euler.z = desiredZRotation;
                targetRotation = Quaternion.Euler(euler);

                // as long as rotation won't completely flip to other side
                if ((euler.z > -80 && euler.z < 80) || noRotateLimits) // if more than 80 degrees in either direction
                {
                    StartCoroutine(slerpBodyRotateToPlayer(targetRotation));
                    
                    // gives permission to leg to expand and attack once main body rotated properly
                    closestLeg.ableToAttack = true;
                }
                // if the rotation does rotate excessively, cancel the current attack
                else
                {
                    isAttacking = false;
                    rotateToZero = false;
                    isAttackingTimer = 0.0f;
                    closestLeg.ableToAttack = false;
                }
            }
            else
            {
                isAttacking = false;
                rotateToZero = false;
            }
        }




    }

    IEnumerator slerpBodyRotateToPlayer(Quaternion correctedTargetRotation)
    {
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        while (elapsedTime < 0.4f) // want it to rotate within 0.1 seconds
        {
            // Interpolate between the start rotation and the target rotation
            Quaternion newRotation = Quaternion.Slerp(startRotation, correctedTargetRotation, elapsedTime / 0.4f);

            // Apply the new rotation
            transform.rotation = newRotation;

            // Update elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final rotation is exactly the target rotation
        transform.rotation = correctedTargetRotation;
    }

    IEnumerator rotateBackToZero()
    {
        rotateToZero = true;
        yield return new WaitForSeconds(isAttackingCooldown/4);
        transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * 10f);
    }

    void RecountLegs()
    {

        foreach (MechWeaverLeg leg in legArray)
        { 
            if (leg.health <= 0f && leg.isAlive)
            {
                leg.isAlive = false;
                leg.isAttacking = false;
                leg.ableToAttack = false;
                //Destroy(leg.LegArm.GetComponent<HingeJoint>()); // for making leg dramatically look like its falling, but then have to change how reset MechWeaver
                StartCoroutine(leg.destroyLeg());
                legCount--;
                Debug.Log("Leg: " + leg.name + " " + legCount + " " + currentPhase);
            }

            //if (leg == null)
            //{
            //    Debug.Log("Leg Removed");
            //    legArray.Remove(leg);
            //    currentLegCount--;
            //}

        }

    }

    void bulletSpreadAttack()
    {
        if (bulletSpreadTimer > 0.0f)
        {
            bulletSpreadTimer -= Time.deltaTime;
        }
        else
        {
            bulletSpreadTimer = bulletSpreadCooldown / bulletSpreadPulse; // bullet timer will reduce with each added spread

            // spawns a bullet in each direction around the mechWeaver
            foreach (Transform spawnPoint in bulletSpawnPoints)
            {
                Debug.DrawRay(spawnPoint.position, spawnPoint.up * 2, Color.white, 1f);

                GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
                bullet.GetComponent<Bullet>().targetPosition = spawnPoint.up * 2;
                bullet.GetComponent<Bullet>().speed = 4f;
                bullet.GetComponent<Bullet>().targetTag = "Player";

                //if (bullet != null)
                //{
                //    Destroy(bullet, 2f);
                //}
            }

            // seconadry bullet spread at phase 4
            if (currentPhase == 4)
            {
                StartCoroutine(secondaryBulletSpread());
            }

        }
    }

    IEnumerator secondaryBulletSpread()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (Transform spawnPoint in bulletSpawnPointsSecondary)
        {
            Debug.DrawRay(spawnPoint.position, spawnPoint.up * 2, Color.white, 1f);

            GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            bullet.GetComponent<Bullet>().targetPosition = spawnPoint.up * 2;
            bullet.GetComponent<Bullet>().speed = 4f;
            bullet.GetComponent<Bullet>().targetTag = "Player";

            //if (bullet != null)
            //{
            //    Destroy(bullet, 2f);
            //}
        }
    }

    void spawnDrones()
    {
        if (currentDrones < maxDrones && droneSpawnTimer <= 0.0f)
        {
            droneSpawnTimer = droneSpawnCooldown;

            GameObject mechWeaverDrone = Instantiate(mechWeaverDronePrefab, droneSpawnPoint.position, Quaternion.identity);
            mechWeaverDrone.GetComponent<MechWeaverDrone>().player = player;
            mechWeaverDrone.GetComponent<MechWeaverDrone>().parentMechWeaver = gameObject.GetComponent<MechWeaverAgent>();
            currentDrones++;
        }

        droneSpawnTimer -= Time.deltaTime;

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


    void resetPosition()
    {
        isResetting = true;
        isAttacking = false;

        // Moves MechWeaver back to center of the arena (spawn)
        //Vector3 directionToSpawn = (transform.position - spawn).normalized;
        //Vector3 move = directionToSpawn * moveSpeed * Time.deltaTime;

        //rb.MovePosition(rb.position + move);
        StartCoroutine(moveToSpawn());
        transform.rotation = new Quaternion(0, 0, 0, 1); // reset rotation

        // phase 4 is locked to center, so will not enable movement if on phase 4
        if (currentPhase < 4)
        {
            StartCoroutine(noLongerResetting());
        }
    }

    IEnumerator moveToSpawn()
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        float moveToSpawnTime = 1f;

        while (elapsedTime < moveToSpawnTime) // want it to reset within moveToSpawnTime seconds
        {
            // Interpolate between the start rotation and the target rotation
            Vector3 newPosition = Vector3.Slerp(startPosition, spawn, elapsedTime / moveToSpawnTime);
            
            // Apply the new rotation
            transform.position = newPosition;

            // Update elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final rotation is exactly the target rotation
        transform.position = spawn;
    }

    IEnumerator noLongerResetting()
    {
        yield return new WaitForSeconds(resetCooldown);

        isResetting = false;
    }

    IEnumerator phase1StartTime()
    {
        yield return new WaitForSeconds(3.0f);

        isResetting = false;
    }

    float currentTotalHealth()
    {
        float currentTotalHealth = health; // start with MechWeaver body health

        if (legCount <= 0)
        {
            return currentTotalHealth;
        }

        foreach (MechWeaverLeg leg in legArray)
        {
            if (leg.isAlive)
            {
                currentTotalHealth += leg.health;
            }
        }

        return currentTotalHealth;
    }

    IEnumerator destroyMechWeaver()
    {
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("BulletPlayer"))
        {
            //hitCounter += 1; // implemented in BulletPlayer
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
        }
        else if (collision.collider.CompareTag("Player"))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
        }
    }

}
