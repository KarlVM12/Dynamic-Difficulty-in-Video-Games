using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.MLAgents;
using UnityEngine.InputSystem;

public class PlayerGun : MonoBehaviour
{
    //public Transform target; // Sentry Gun's Transform
    public float rotationSpeed = 5f;
    private Quaternion originalRotation;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float machineGunBulletCooldown = 0.15f;
    public float machineGunBurstCooldown = 0.1f;
    public float range = 12f;

    [SerializeField] ParticleSystem muzzleFlash;

    private float machineGunBurstTimer;

    private GameObject currentClosestEnemy;

    void Start()
    {
        originalRotation = transform.rotation;

        currentClosestEnemy = null;
    }


    void Update()
    {
        currentClosestEnemy = determineClosestEnemy();

        // Uncomment in AlexGameplay with active UI controls (uncomment last closing bracket too !!)
        //  if (Gamepad.current.rightTrigger.isPressed || Input.GetMouseButtonDown(0))
        if (Gamepad.current != null && Gamepad.current.rightTrigger.isPressed)
        {
            if (currentClosestEnemy == null) // if no enemy to shoot at, just shoot straight
            {
                // clips gun back to original position so it shoots straight instead of last rotation it was shooting in
                transform.rotation = originalRotation;

                // Machine Gun attack
                if (Time.time >= machineGunBurstTimer)
                {
                    ShootMachineGun(bulletSpawnPoint.right * 2);
                    machineGunBurstTimer = Time.time + machineGunBurstCooldown;
                }
            }
            if (currentClosestEnemy != null)
            {
                Vector3 targetDirection = currentClosestEnemy.transform.position - transform.position;

                if (isEnemyInLineOfSight(targetDirection)) //isEnemyInLineOfSight(targetDirection)
                {
                    // in case turret has rotated away from target, for a smooth rotation towards target so turret doesn't just snap to target
                    Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

                    Debug.DrawRay(transform.position, targetDirection, Color.blue);
                }
                // Machine Gun attack
                if (Time.time >= machineGunBurstTimer)
                {
                    ShootMachineGun(targetDirection);
                    machineGunBurstTimer = Time.time + machineGunBurstCooldown;
                }
            }
        }
    }

    GameObject determineClosestEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range);

        // Count the number of enemies within range
        List<GameObject> enemiesInRange = new List<GameObject>();
        foreach (Collider collider in colliders)
        {
            // check if an enemy as well as see if it is the parent enemy object since only the parent is of type Agent (or MechWeaverLegCapsuleCollider in the case of the MechWeaver fight)
            // if (collider.CompareTag("Enemy") && (collider.gameObject.GetComponent<Agent>() != null || collider.gameObject.GetComponent<MechWeaverLegCapsuleCollider>() != null) && collider.gameObject != gameObject)
            if (collider.CompareTag("Enemy") && (collider.gameObject.GetComponent<EnemyInterface>() != null || collider.gameObject.GetComponent<MechWeaverLegCapsuleCollider>()) && collider.gameObject != gameObject)
            {
                enemiesInRange.Add(collider.gameObject);
            }
        }

        // determine the closest enemy to player
        GameObject closestEnemy = null;
        float minDistanceToPlayer = Mathf.Infinity;
        foreach (GameObject enemy in enemiesInRange)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToPlayer < minDistanceToPlayer)
            {
                minDistanceToPlayer = distanceToPlayer;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }


    bool isEnemyInLineOfSight(Vector3 direction)
    {
        // if raycast hits enemy in range, then can start shooting
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, range))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                return true;
            }
        }

        return false;
    }

    void ShootMachineGun(Vector3 guessTarget)
    {
        //StartCoroutine(FireBullets());
        SpawnBullet(guessTarget);
        muzzleFlash.Play();
    }

    IEnumerator FireBullets(Vector3 guessTarget)
    {
        // Intermittent Rapid Fire (5 Bullets, small delay between each)
        for (int i = 0; i < 5; i++)
        {
            SpawnBullet(guessTarget);
            yield return new WaitForSeconds(machineGunBulletCooldown);
        }
    }

    void SpawnBullet(Vector3 guessTarget)
    {
        // Calculate the direction and position to the target
        Vector3 directionToTarget = guessTarget - bulletSpawnPoint.position;
        Vector3 targetPosition = bulletSpawnPoint.position + directionToTarget;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Assign the target position and tag to the bullet
        bullet.GetComponent<BulletPlayer>().targetPosition = targetPosition;
        bullet.GetComponent<BulletPlayer>().targetTag = "Enemy";

        if (bullet != null)
        {
            Destroy(bullet, 0.5f);
        }

    }
}
