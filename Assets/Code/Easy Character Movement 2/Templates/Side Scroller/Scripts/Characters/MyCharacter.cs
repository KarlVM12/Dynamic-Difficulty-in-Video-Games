using System;
using UnityEngine;

namespace EasyCharacterMovement.Templates.SideScrollerTemplate
{
    public class MyCharacter : Character
    {

        float movementDirection;

        public Boolean WalkThroughDoor;

        public int hitCounter = 0;
        public float health = 100;

        public ProgressBar HealthBar;

        public Vector3 lastCheckpoint;

        public GameObject currentLevelHandler;

        protected override void Start()
        {
            base.Start();

            CustomStart();
            
        }

        private void CustomStart()
        {

            HealthBar.BarValue = health;

        }

        protected override void Update()
        {
            base.Update();

            CustomUpdate();
        }

        
        private void CustomUpdate()
        {
            Debug.Log("working towards it");
            if (WalkThroughDoor)
            {
                Debug.Log("it was set!!");
                movementDirection = 0.35f;
                SetMovementDirection(Vector3.right * movementDirection);
                SetYaw(90.0f);
            }

            // Karl: the progress was not updating so i put this here instead of just in start
            if (health >= 0)
            {
                HealthBar.BarValue = health;

                // Therefore, it will display the 0 health, and then compare the float and bring it down so the player will die next frame instead of maybe being caught on a decimal value
                if (health > -1f && health < 1f)
                {
                    health = -1;
                }
            }
            else
            {
                // should implement the player character dying animation as well as where to reset/respawn here i.e.:
                // playerAlive = false;
                // playerAnimator.play("Death:);
                // ...
                // player.transform = checkpoints[lastCheckpoint];
             
                Debug.Log(characterMovement.transform.position + "myccharacter");
                Debug.Log(transform.position + "player");

                //transform.position = lastCheckpoint.transform.position;
                characterMovement.position = lastCheckpoint;
                Debug.Log(characterMovement.transform.position + "mycharatcer");
                Debug.Log(transform.position + "player");
                health = 100.00f;

                currentLevelHandler.GetComponent<LevelHandler>().resetLevel();
               
            }
        }

        protected override void HandleInput()
        {
            // Should handle input ?

            if (inputActions == null)
                return;

            // Add horizontal input movement (in world space)
            float movementDirection1D = 0.0f;

            Vector2 movementInput = GetMovementInput();
            if (movementInput.x > 0.0f)
                movementDirection1D = 1.0f;
            else if (movementInput.x < 0.0f)
                movementDirection1D = -1.0f;

            SetMovementDirection(Vector3.right * movementDirection1D);

            // Snap side to side rotation

            if (movementDirection1D != 0.0f)
                SetYaw(movementDirection1D * 90.0f);
        }

        protected override void OnOnEnable()
        {
            // Call base method implementation

            base.OnOnEnable();

            // Disable character rotation

            SetRotationMode(RotationMode.None);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Bullet"))
            {
                hitCounter += 1;
            }
        }

    }
}
