
using UnityEngine;

public class BulletPlayer : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 targetPosition;
    public string targetTag; // will use tags to deal with different types of objects

    void Start()
    {
        // Calculate the direction to the target position
        Vector3 directionToTarget = targetPosition;// - transform.position;

        // Set the velocity based on the calculated direction
        GetComponent<Rigidbody>().velocity = directionToTarget.normalized * speed;
    }



    void Update()
    {

    }

    void OnTriggerEnter(Collider collider)
    {

        if (collider.CompareTag("Bullet") || collider.CompareTag("BulletPlayer") || collider.CompareTag("Player"))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collider);
            return;
        }

        if (targetTag == "Enemy")
        {
            if (collider.CompareTag("Enemy"))
            {
                if (collider.TryGetComponent(out EnemyInterface enemy))
                {
                    // Enemy takes damage
                    enemy.health -= 5;
                    enemy.hitCounter += 1;

                    Destroy(gameObject);
                    return;
                }

            }
            //Debug.Log("PlayerBullet Collision with Tag: " + collision.collider.tag);
        }

        Destroy(gameObject);


    }
}
