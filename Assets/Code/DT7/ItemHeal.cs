using EasyCharacterMovement.Templates.SideScrollerTemplate;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemHeal : MonoBehaviour
{
    [SerializeField] float healthIncrease = 50;
   
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if(collider.gameObject.GetComponent<MyCharacter>().health + healthIncrease <= 100f) { 
                collider.gameObject.GetComponent<MyCharacter>().health += healthIncrease;
               }
            else
            {
                collider.gameObject.GetComponent<MyCharacter>().health = 100f;
            }
            this.gameObject.SetActive(false);
        }
    }
}
