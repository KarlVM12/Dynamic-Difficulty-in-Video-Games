using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using EasyCharacterMovement.Examples.Events.CharacterEventsExample;
using UnityEngine.InputSystem.OnScreen;
using Unity.VisualScripting;

public class tutorialHandler : MonoBehaviour
{

    //gameobjects
    [SerializeField] TextMeshProUGUI txtStory;
    [SerializeField] float characterTime;
    [SerializeField] float imageFadeSpeed;

    [SerializeField] BoxCollider doorAcess;

    [SerializeField] Button buttonJump;
    [SerializeField] Button buttonSprint;
    [SerializeField] Button buttonCrouch;
    [SerializeField] Button buttonShoot;

    public Image LeftJoystickHandle;

    Image imageJump;
    Image imageSprint;
    Image imageCrouch;
    Image imageShoot;

    Image CurrentFading = null;

    Boolean fading = true;

    //rate at which the text flies across the screen
    float baseTime;

    //strings that the user would like the tutorial to run through
    public string[] strArray;

    //handles the stages of the tutorial
    int i = 0;
    
    void Start()
    {
        imageJump = buttonJump.GetComponent<Image>();
        imageSprint = buttonSprint.GetComponent<Image>();
        imageCrouch = buttonCrouch.GetComponent<Image>();
        imageShoot = buttonShoot.GetComponent<Image>();

        buttonShoot.interactable = false;
        buttonJump.interactable = false;
        buttonSprint.interactable = false;
        buttonCrouch.interactable = false;
        
        buttonCrouch.gameObject.GetComponent<OnScreenButton>().enabled = false;
        buttonSprint.gameObject.GetComponent<OnScreenButton>().enabled = false;
        buttonJump.gameObject.GetComponent<OnScreenButton>().enabled = false;
        buttonShoot.gameObject.GetOrAddComponent<OnScreenButton>().enabled = false;

        CurrentFading = LeftJoystickHandle;
        EndCheck();
    }

    void Update()
    {
        Debug.Log(CurrentFading);
       //controls the fading in and out for indication of which button the tutorial would like the player to interact with
        if (CurrentFading != null)
        {
            if (fading)
            {
                float alphaValue = Time.deltaTime * imageFadeSpeed;


                Color color = CurrentFading.color;

                color.a -= alphaValue;

                CurrentFading.color = color;
                if (color.a <= 0.0f)
                {
                    fading = false;
                }
            }
            else if (fading == false)
            {
                float alphaValue = Time.deltaTime * imageFadeSpeed;


                Color color = CurrentFading.color;

                color.a += alphaValue;

                CurrentFading.color = color;

                if (color.a >= 1.0f)
                {
                    fading = true;
                }
            }
        }



        //listens for the control being pressed tthat is associated with each conncurrent stage of the tutorial
        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.IsActuated() && i == 0)
            {
                buttonSprint.interactable = true;
                buttonSprint.gameObject.GetComponent<OnScreenButton>().enabled = true;
                Invoke("EndCheck", 0);
                i++;
                CurrentFading = imageSprint;
                Color reset = LeftJoystickHandle.color;
                reset.a = 1.0f;
                LeftJoystickHandle.color = reset;
            }
            else if (Gamepad.current.leftTrigger.IsPressed() && i == 1)
            {
                buttonCrouch.interactable = true;
                buttonCrouch.gameObject.GetComponent<OnScreenButton>().enabled = true;
                routineHandler();
                Invoke("EndCheck", 0);
                i++;
                CurrentFading = imageCrouch;
                Color reset = imageSprint.color;
                reset.a = 1.0f;
                imageSprint.color = reset;
            }
            else if (Gamepad.current.buttonEast.IsPressed() && i == 2)
            {
                buttonJump.interactable = true;
                buttonJump.gameObject.GetComponent<OnScreenButton>().enabled = true;
                routineHandler();
                Invoke("EndCheck", 0);
                i++;
                CurrentFading = imageJump;
                Color reset = imageCrouch.color;
                reset.a = 1.0f;
                imageCrouch.color = reset;
            }
            else if (Gamepad.current.buttonSouth.IsPressed() && i == 3)
            {
                buttonShoot.interactable = true;
                buttonShoot.gameObject.GetComponent<OnScreenButton>().enabled = true;
                routineHandler();
                Invoke("EndCheck", 0);
                i++;
                CurrentFading = imageShoot;
                Color reset = imageJump.color;
                reset.a = 1.0f;
                imageJump.color = reset;
            }
            else if (Gamepad.current.rightTrigger.IsPressed() && i == 4)
            {
                routineHandler();
                Invoke("EndCheck", 0);
                i++;
                CurrentFading = null;
                Color reset = imageShoot.color;
                reset.a = 1.0f;
                imageShoot.color = reset;
                doorAcess.enabled = true;
            }
        }
    }

   //Handles the coroutines in a way so that they dont interfere with each other if the previous is not quite yet completed
 private Coroutine textVisibleCoroutine;
    void routineHandler()
    {
        if (textVisibleCoroutine != null) {
            StopCoroutine(textVisibleCoroutine);
        }

        textVisibleCoroutine = StartCoroutine(TextVisible());
    }

    //makes sure that there is even another available string in the unity defined string array
    void EndCheck()
    {
        if (i <= strArray.Length - 1)
        {
            txtStory.text = strArray[i];
            routineHandler();
        }
    }


    //handles the writing of the characters across the screen
    private IEnumerator TextVisible()
    {
        txtStory.ForceMeshUpdate();
        int totalVisibleCharacters = txtStory.textInfo.characterCount;
        int counter = 0;


        while (true)
        {
            int visibleCount = counter % (totalVisibleCharacters + 1);

            txtStory.maxVisibleCharacters = visibleCount;

            if (visibleCount >= totalVisibleCharacters)
            {
                
                break;
            }
            counter++;

            yield return new WaitForSeconds(characterTime);
        }
    
    }
}