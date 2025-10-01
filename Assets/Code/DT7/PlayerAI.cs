using EasyCharacterMovement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerAI : MonoBehaviour
{
    private CharacterMovement characterController;

    [SerializeField] private Transform movePositionTransform;
    private NavMeshAgent navMeshAgent;

    private float jumpTimer;
    private float jumpCooldown = 2f;

    public int hitCounter = 0;

    public float health = 100;

    void Awake()
    {
        characterController = GetComponent<CharacterMovement>();

        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    
    void Update()
    {

        //navMeshAgent.destination = movePositionTransform.position;

        // Every couple of seconds, 50% chance to jump
        if (Time.time >= jumpTimer)
        {
            jumpTimer = Time.time + jumpCooldown;
            float randomValue = (float)Random.Range(1, 3);
            if ((int)randomValue == 1)
            {
                if (characterController.isOnGround)
                {
                    //Debug.Log("Trying to jump");
                    characterController.PauseGroundConstraint();
                    characterController.LaunchCharacter(Vector3.up * 6.0f);
                    characterController.Move();
                }
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            hitCounter += 1;
        }
    }
}
