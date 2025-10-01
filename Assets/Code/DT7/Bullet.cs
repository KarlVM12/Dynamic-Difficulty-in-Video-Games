using EasyCharacterMovement.Templates.SideScrollerTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 targetPosition;
    public string targetTag; // will use tags to deal with different types of objects

    void Start()
    {
        // Calculate the direction to the target position
        Vector3 directionToTarget = targetPosition;// - transform.position;

        // Set the velocity based on the calculated direction (need to normalized so distance doesn't increase speed)
        GetComponent<Rigidbody>().velocity = directionToTarget.normalized * speed;
    }


    
    void Update()
    {
    
    }  

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Bullet") || collider.CompareTag("BulletPlayer"))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collider);
            return;
        }


        if (targetTag == "Player")
        {

            if (collider.CompareTag("Enemy"))
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), collider);
                return;
            }

            if (collider.CompareTag("Player"))
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), collider);
                //collision.collider.GetComponent<PlayerAI>().hitCounter += 1; // already handled in the player script
                if (collider.GetComponent<MyCharacter>() != null)
                { 
                    collider.GetComponent<MyCharacter>().health -= 2; // machine gun bullets deal 2 damage

                    //Debug.Log("Hit Player, Current Health: " + collision.collider.GetComponent<MyCharacter>().health);
                } 
                Destroy(gameObject);
                return;
            }
        }

        Destroy(gameObject);


    }
}
