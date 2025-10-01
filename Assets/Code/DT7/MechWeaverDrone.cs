using EasyCharacterMovement.Templates.SideScrollerTemplate;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class MechWeaverDrone : MonoBehaviour, EnemyInterface
{
    // ===| MechWeaverDrone Logic |===
    // When spawns, falls to the ground
    // Once it hits the ground, its only movement is back and forth, towards the direction of the player
    // Has low health: 15
    // if hits player, damages for 2, only once a second, not every frame
    // collider trigger

    private Rigidbody rb;

    public MechWeaverAgent parentMechWeaver;
    public MyCharacter player;

    public float maxHealth { get; set; } = 0;
    public float health { get; set; }
    public int hitCounter { get; set; } = 0;
    public Vector3 spawn { get; set; }

    private float moveSpeed;

    private bool isFalling;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        maxHealth = 15f;
        health = maxHealth;

        moveSpeed = 5f;
        
        isFalling = true;


    }


    void Update()
    {
        if (transform.position.y < (transform.localScale.y / 2f) && isFalling) // transform.localScale.y = height, anchor is in center of drone, so if higher than half (changed from / 2f--> / 1.5f), means it is off the ground
        {
            isFalling = false;
            //rb.useGravity = false;
            //rb.isKinematic = true;
            //Debug.Log("stopped falling");
        }


        if (!isFalling)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            Vector3 move = directionToPlayer * moveSpeed * Time.deltaTime;
            move = new Vector3(move.x, 0f, 0f);

            rb.MovePosition(rb.position + move);
        }

        if (health <= 0f)
        {
            if (parentMechWeaver != null)
            {
                parentMechWeaver.GetComponent<MechWeaverAgent>().currentDrones--;
            }
            
            Destroy(gameObject);
        }

        if (player.health <= 0f)
        {
            Destroy(gameObject);
        }

    }

    //void OnTriggerEnter(Collider collider)
    //{

    //    if (collider.CompareTag("Player"))
    //    {
    //        if (collider.GetComponent<MyCharacter>() != null)
    //        {
    //            // every time the player is touching the MechWeaverDrone, they lose 2 health
    //            collider.GetComponent<MyCharacter>().health -= 1f;
    //        }
    //    }
    //    //else if (collider.CompareTag("BulletPlayer")) // handled in BulletPlayer since it is of type EnemyInterface
    //    //{
    //    //    health -= 5f;
    //    //}
    //    else
    //    {
    //        Physics.IgnoreCollision(GetComponent<Collider>(), collider);
    //        return;
    //    }
    //}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (collision.collider.GetComponent<MyCharacter>() != null)
            {
                // every time the player is touching the MechWeaverDrone, they lose 2 health
                collision.collider.GetComponent<MyCharacter>().health -= .25f;
            }
        }
        //else if (collider.CompareTag("BulletPlayer")) // handled in BulletPlayer since it is of type EnemyInterface
        //{
        //    health -= 5f;
        //}
        else
        {
            //Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            return;
        }
    }
}
