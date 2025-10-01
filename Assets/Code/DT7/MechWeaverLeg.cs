using EasyCharacterMovement.Templates.SideScrollerTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechWeaverLeg : MonoBehaviour, EnemyInterface
{
    [SerializeField] public Transform target;

    public float health { get; set; } = 0;
    public float maxHealth { get; set; } = 50f;
    public int hitCounter { get; set; } = 0;
    
    public Vector3 spawn { get; set; }
    
    [SerializeField] float damage;

    [SerializeField] float range;
    [SerializeField] float chargeTime;
    [SerializeField] float cooldownTime;
    private float attackTimer;

    public bool isAttacking = false;
    public bool ableToAttack = false;

    private bool playerInSight = false;

    public Transform LegArm;
    public Transform LegArmVisionPoint;

    public Transform LegForearm;
    public Transform LegForearmVisionPoint;

    private float maxHingeLimit;

    public float distanceToPlayer;

    private Vector3 initialForearmScale;
    private Vector3 initialForearmPosition;

    public bool isAlive;

    void Start()
    {
        maxHealth = 50;
        health = maxHealth;
        isAlive = true;

        LegArm = transform.GetChild(0).GetChild(0);
        LegArmVisionPoint = LegArm.GetChild(0);

        LegForearm = transform.GetChild(0).GetChild(1).GetChild(0);
        LegForearmVisionPoint = LegForearm.GetChild(0);

        maxHingeLimit = LegForearm.GetComponent<HingeJoint>().limits.max;

        initialForearmScale = LegForearm.localScale;
        initialForearmPosition = LegForearm.position;

        attackTimer = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {
        // Inside RecountLegs() in MechWeaverAgent
        //if (health <= 0f)
        //{
        //    if (LegArm.GetComponent<HingeJoint>() != null)
        //    {
        //        Destroy(LegArm.GetComponent<HingeJoint>());
        //        LegArm.GetComponent<Collider>().isTrigger = false;
        //        // maybe make this a coroutine to wait another second to destroy entire arm
        //        StartCoroutine(destroyLeg());
        //    }
        //}



        // should subtract it by end of LegForearm, should get these in start (Future Karl: why did i say to get these in start?)
        Vector3 playerPos = (target.transform.position + (new Vector3(0f, 0.7f, 0f)));
        Vector3 MechWeaverLegDirection = playerPos - LegForearmVisionPoint.position;
        
        IsPlayerInLineOfSight(MechWeaverLegDirection);

        distanceToPlayer = Vector3.Distance(playerPos, LegForearmVisionPoint.position);

        // if player is in sight, attempt to attack, but it has to check if it is allowed to attack
        // basically, the second one leg detects the player, has to let main script know, so it becomes able to attack and goes forward with attacking
        // the main script will constantly be checking if any legs are starting to attack to which it will then say that first one that isAttacking will become ableToAttack
        // this will force all other legs to set isAttacking to false as well as ableToAttack until 0.5 seconds have passed, even tho that leg might have a cooldown.
        //  have to factor in cooldown in able to attack as well, if it is on cooldown and tries to attack (isAttacking = true), will move onto next leg


        // cooldown timer
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            isAttacking = false;
        }

        if (attackTimer <= 0.0001) // for float inaccuracy
        {
            // scale back extension of legForearm
            Vector3 startingForearmScale = LegForearm.localScale;
            LegForearm.localScale = Vector3.Slerp(startingForearmScale, initialForearmScale, Time.deltaTime * 5f);
            //Vector3 startingForearmPosition = LegForearm.position;
            //LegForearm.position = Vector3.Lerp(startingForearmPosition, initialForearmPosition, Time.deltaTime * 5f);

            // StartCoroutine(changeYScale(0.3f)); // should not really be calling this each frame

            // set the hinge of the LegForearm back to original range
            HingeJoint legForearmHingeJoint = LegForearm.GetComponent<HingeJoint>();
            JointLimits newLimits = legForearmHingeJoint.limits;
            newLimits.max = maxHingeLimit;

            legForearmHingeJoint.limits = newLimits;


            if (playerInSight)
            {
                isAttacking = true;
            }
            else
            {
                isAttacking = false;
            }
        }

        if (isAttacking && ableToAttack) 
        {
            StartCoroutine(ChargeLegAttack());
            ableToAttack = false;
        }
    }

    void IsPlayerInLineOfSight(Vector3 direction) 
    {
        // Debug.DrawRay(transform.GetChild(0).position, direction, Color.yellow);
        // if raycast hits player in range, then can start shooting
        RaycastHit hit;
        Debug.DrawRay(LegForearmVisionPoint.position, direction.normalized * range, Color.red);

        if (Physics.Raycast(LegForearmVisionPoint.position, direction, out hit, range)) // , LayerMask.GetMask("Player")
        {
            if (hit.collider.CompareTag("Player"))
            {
                //return true;
                playerInSight = true;
                Debug.DrawRay(LegForearmVisionPoint.position, direction.normalized * range, Color.green);
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

    IEnumerator ChargeLegAttack()
    {
        yield return new WaitForSeconds(chargeTime);

        LegAttack();
    }


    void LegAttack()
    {
        // physics for elongating each part of the leg and moving it towards the player, maybe call to the parent to rotate to the player as well
        // rotates whole body towards direction
        // straightens leg
        // extends leg towards player to hit

        //Debug.Log(LegArm.parent.parent.name + " Attacked! " + Time.time);
        // charge time

        // clamp the hinge of LegForearm to be straight along LegArm
        HingeJoint legForearmHingeJoint = LegForearm.GetComponent<HingeJoint>();
        JointLimits newLimits = legForearmHingeJoint.limits;
        newLimits.max = newLimits.min + 0.1f;
        
        legForearmHingeJoint.limits = newLimits;

        // moving the position of the leg by this amount is not really necessary anymore, should really get rid of it
        LegForearm.position = new Vector3(LegForearm.position.x + 0.318f, LegForearm.position.y - 0.373f, LegForearm.position.z);
        
        // extend the length of LegForearm
        StartCoroutine(changeYScale(0.8f));

        // add cooldown
        attackTimer = cooldownTime;

    }

    IEnumerator changeYScale(float newYScale)
    {
        Vector3 startScale = LegForearm.localScale;
        Vector3 targetScale = new Vector3(startScale.x, newYScale, startScale.z);

        // have to move the position to the end of LegArm since expansion will go into LegArm
        //Vector3 startPos = LegForearm.position;
        //Vector3 targetPos = new Vector3(startPos.x + 0.318f, startPos.y - 0.373f, startPos.z);

        float elapsedTime = 0f;
        while (elapsedTime < 0.1f) // want it to rotate within 0.1 seconds
        {
            // Interpolate between the start scale/position and the target scale/position
            Vector3 newScale = Vector3.Slerp(startScale, targetScale, elapsedTime / 0.1f);
            //Vector3 newPos = Vector3.Slerp(startPos, targetPos, elapsedTime / 0.1f);

            // Apply the new scale/position
            LegForearm.localScale = newScale;
            //LegForearm.position = newPos;

            // Update elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the final rotation is exactly the target scale/position
        LegForearm.localScale = targetScale;
        //LegForearm.position = targetPos;

        yield return null;
    }

    // destroys current leg after one second
    public IEnumerator destroyLeg()
    {
        yield return new WaitForSeconds(1f);

        //Destroy(gameObject);
        isAttacking = false;
        ableToAttack = false;
        playerInSight = false;
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider collider)
    {
        // parent script has no collider
        //if (collider.CompareTag("Player"))
        //{
        //    if (collider.GetComponent<MyCharacter>() != null)
        //    {
        //        // every time the player is touching a leg, they lose health
        //        collider.GetComponent<MyCharacter>().health -= 8;
        //    }
        //}
    } 
}
