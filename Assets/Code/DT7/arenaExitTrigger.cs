using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class arenaExitTrigger : MonoBehaviour
{

    [SerializeField] DT7.CharacterMovement.Camera.CameraController mainCamera;
    private bool enteredOnce = false;

    [SerializeField] UnityEngine.UI.Image fadeToBlack;
    private bool leaving = false;

    private void Update()
    {
        // black fade out on exit, and then maybe go to menu?
        if (leaving)
        {
            Color fading = fadeToBlack.color;

            if (fading.a <= 1f)
            {
                fading.a += 0.5f * Time.deltaTime;
                fadeToBlack.color = fading;
            }
            else
            {
                leaving = false;
            }
        }
    }


    void OnTriggerEnter(Collider collider)
    {
        if (!enteredOnce && collider.CompareTag("Player"))
        {
            // doesn't work for all devices
            //mainCamera.target = arenaCenterTransform;
            //mainCamera.distanceToTarget = 15.75f;
            mainCamera.distanceToTarget = 4f;

            enteredOnce = true;
            leaving = true;

            StartCoroutine(backToMainMenu());
        }
    }

    IEnumerator backToMainMenu()
    {
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("menu");
    }
}
