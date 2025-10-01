using EasyCharacterMovement.Templates.SideScrollerTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;

public class LevelHandlerBossOne : MonoBehaviour, LevelHandler
{

    [SerializeField] public DT7.CharacterMovement.Camera.CameraController mainCamera;
    
    [SerializeField] public GameObject exitDoorTrigger;
    [SerializeField] public MechWeaverAgent mechWeaver;
    [SerializeField] public MyCharacter player;


    private bool starting = true;
    private bool resetting = false;
    [SerializeField] UnityEngine.UI.Image fadeToBlack;
    
    [SerializeField] public GameObject screenCover;

    [SerializeField] public GameObject itemHeal;
    private float healthTimer = 0.0f;
    private float healthCooldownTime = 20f;
    private bool itemHealPickedUp = false;


    void Awake()
    {
        fadeToBlack.color = Color.black;
    }


    void Update()
    {
        // black fade in on start
        if (starting)
        {
            Color fading = fadeToBlack.color;

            if (fading.a >= 0f)
            {
                fading.a -= 0.5f * Time.deltaTime;
                fadeToBlack.color = fading;
            }
            else
            {
                starting = false;
            }
        }

        if (mechWeaver != null && mechWeaver.health <= 0)
        {
            // activate exit door trigger
            exitDoorTrigger.SetActive(true);
            
            mainCamera.target = player.transform;
            mainCamera.distanceToTarget = 4f;

            screenCover.SetActive(false);

        }

        if (player.health <= 0)
        {
            // reset level
            // resetLevel();

            if (!resetting)
            {
                resetting = true;

                player.health = 0f; // keeps player health bar at zero
                player.HealthBar.BarValue = 0f;
                player.gameObject.SetActive(false);

                StartCoroutine(resetScene());
            }
        }

        if (resetting)
        {
            // fades to black on player death
            Color fading = fadeToBlack.color;

            if (fading.a <= 1f)
            {
                fading.a += 1f * Time.deltaTime;
                fadeToBlack.color = fading;
            }
            else
            {
                resetting = false;
            }
        }

        //  Health will spawn every [healthCooldownTime] seconds
        if (!itemHeal.activeSelf && !itemHealPickedUp)
        {
            itemHealPickedUp = true;
            healthTimer = healthCooldownTime;
        }

        if (healthTimer >= 0.0f)
        {
            healthTimer -= Time.deltaTime;
        }
        else
        {
            itemHeal.SetActive(true);
            itemHealPickedUp = false;
        }

    }
    public void resetLevel()
    {
        // reset MechWeaver status, health, and location
        // reset player location? probably done in MyCharacter
        //mechWeaver.transform.position = mechWeaver.spawn;
        //mechWeaver.transform.rotation = new Quaternion(0, 0, 0, 0);
        
        //foreach (Transform leg in mechWeaver.transform)
        //{
        //    if (leg.GetComponent<MechWeaverLeg>() != null)
        //    {
        //        leg.GetComponent<MechWeaverLeg>().health = leg.GetComponent<MechWeaverLeg>().maxHealth;
        //        leg.GetComponent<MechWeaverLeg>().isAlive = true;
        //        leg.gameObject.SetActive(true);
        //    }
        //}

        //mechWeaver.isResetting = true;
        //mechWeaver.currentPhase = 1;
        //mechWeaver.Start();
        //mechWeaver.health = mechWeaver.maxHealth;
    }

    IEnumerator resetScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
