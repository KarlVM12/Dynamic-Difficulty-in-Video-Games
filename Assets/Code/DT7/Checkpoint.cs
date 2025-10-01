using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] EasyCharacterMovement.Templates.SideScrollerTemplate.MyCharacter player;
    [SerializeField] GameObject barricade;
    [SerializeField] GameObject hide;
    [SerializeField] GameObject show;
    private void OnTriggerEnter(Collider collider) 
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log("detected a collision with: " + this);
            player.lastCheckpoint = new Vector3(player.GetPosition().x, player.GetPosition().y, 0.0f);
            barricade.SetActive(true);


            Destroy(hide);
            
           
           show.SetActive(true);

           
        }
    }
}
