using EasyCharacterMovement.Templates.SideScrollerTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEnemy : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 targetPosition;
    public string targetTag; // will use tags to deal with different types of objects
    public float damage = 6.0f;

    void Start()
    {
        //MoveCenterToEnd();
        Vector3 directionToTarget = targetPosition;// - transform.position;

        // Set the velocity based on the calculated direction
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
                    collider.GetComponent<MyCharacter>().health -= damage;
                    Destroy(gameObject);

                }
                return;
            }
        }

        Destroy(gameObject);
    }
}
