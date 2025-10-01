using DT7.CharacterMovement;
using EasyCharacterMovement;
using EasyCharacterMovement.Examples.Gameplay.PlanetWalkExample;
using EasyCharacterMovement.Examples.OldInput.SideScrollerExample;
using EasyCharacterMovement.Templates.SideScrollerTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;




    public class doorController : MonoBehaviour
    {
        [SerializeField] GameObject textPrompt;
        [SerializeField] GameObject environment;
        [SerializeField] GameObject pivotPoint;

        [SerializeField] Button useDoor;

        [SerializeField] EasyCharacterMovement.Templates.SideScrollerTemplate.MyCharacter Character;

        public float walkSpeed;

        public OnScreenButton[] buttons;
        public OnScreenStick stick;

        public UnityEvent doorAnimationOpen;
        public UnityEvent doorAnimationClosed;

        public float timeForDoor;

        Transform environmentTransform;
        Transform environmentPivotPoint;

        Boolean canExit;
        Boolean canSpin;
        Boolean canWalk = false;

        float rotationRemaining = 90f;

    public Material newSkybox;

        private void Start()
        {
            environmentTransform = environment.GetComponent<Transform>();
            environmentPivotPoint = pivotPoint.GetComponent<Transform>();
            Character.WalkThroughDoor = false;
        }

        float timeElapsed = 0;
        private void Update()
        {

            //turn the world
            if (canExit && Gamepad.current.buttonWest.IsActuated())
            {
                disableControls();
                canSpin = true;
                if (textPrompt != null)
                {
                    textPrompt.SetActive(false);
                }
                
                canExit = false;
            }


            if (canSpin)
            {
                // Calculate the angle to rotate 90f is rotation speed
                float angleToRotate = 90f * Time.deltaTime;

                environmentTransform.RotateAround(environmentPivotPoint.position, Vector3.up, angleToRotate);

                rotationRemaining -= angleToRotate;

                // If remaining angle is small, complete the rotation instantly
                if (rotationRemaining <= 0)
                {
                    canSpin = false;
                Character.SetPosition(new Vector3(Character.GetPosition().x, Character.GetPosition().y, 0.0f));
                // Ensure exact 90 degree rotation
                environmentTransform.RotateAround(environmentPivotPoint.position, Vector3.up, rotationRemaining);

                    canWalk = true;
                }
            }

            Debug.Log(Character.WalkThroughDoor);

            if (canWalk)
            {
                timeElapsed += 1.0f * Time.deltaTime;

                Character.WalkThroughDoor = true;

                if (timeElapsed >= timeForDoor)
                {
                    Character.WalkThroughDoor = false;
                    Debug.Log("done with this whole door thing");
                    canWalk=false;
                    Character.SetPosition(new Vector3(Character.GetPosition().x, Character.GetPosition().y, 0.0f));
                    Debug.Log(Character.GetPosition());
                    enableControls();
                if(newSkybox != null)
                {
                    changeSky();
                }
                }
            }
        }

    public void changeSky()
    {
        RenderSettings.skybox = newSkybox;
        RenderSettings.sun = null;

        DynamicGI.UpdateEnvironment();
    }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {

                doorAnimationOpen.Invoke();
            }

            //enable the button to go through the door
            useDoor.interactable = true;
        useDoor.gameObject.GetComponent<OnScreenButton>().enabled = true;
            Outline buttonHightlight = useDoor.GetComponent<Outline>();
            TextMeshProUGUI text = useDoor.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "Door";
            buttonHightlight.enabled = true;
            canExit = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {

                doorAnimationClosed.Invoke();

            }

            //disable the button to go through the door
            useDoor.interactable = false;
        useDoor.gameObject.GetComponent<OnScreenButton>().enabled = false;
            Outline buttonHightlight = useDoor.GetComponent<Outline>();
            TextMeshProUGUI text = useDoor.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "";
            buttonHightlight.enabled = false;
            canExit = false;
        }

        private void disableControls()
        {
            stick.enabled = false;
            foreach (OnScreenButton x in buttons)
            {
                x.enabled = false;
            }
        }

        private void enableControls()
        {
            stick.enabled = true;
            foreach (OnScreenButton x in buttons)
            {
                x.enabled = true;
            }
        }
    }

