using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arenaEnteredTrigger : MonoBehaviour
{

    [SerializeField] public Transform arenaCenterTransform;
    [SerializeField] public Transform entranceDoorTriggerTransform;
    [SerializeField] public DT7.CharacterMovement.Camera.CameraController mainCamera;
    //[SerializeField] public Camera arenaCamera;
    [SerializeField] public GameObject screenCover;
    
    [SerializeField] public MechWeaverAgent mechWeaverAgent;
    [SerializeField] public ProgressBar mechWeaverHealthBar;

    private bool enteredOnce = false;

    void OnTriggerEnter(Collider collider)
    {
        if (!enteredOnce && collider.CompareTag("Player"))
        {
            entranceDoorTriggerTransform.GetComponent<BoxCollider>().isTrigger = false; // stops player from walking back through door

            screenCover.SetActive(true);
            
            mechWeaverHealthBar.gameObject.SetActive(true);
            mechWeaverAgent.playerEnteredArena = true;

            mainCamera.target = arenaCenterTransform;
            mainCamera.distanceToTarget = 11f;

            //StartCoroutine(activateArenaCamera());

            enteredOnce = true;
            gameObject.SetActive(false);
        }
    }

    //IEnumerator activateArenaCamera()
    //{
    //    yield return new WaitForSeconds(1);
        
    //    arenaCamera.gameObject.SetActive(true);
    //}
}
