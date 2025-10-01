using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingDeath : MonoBehaviour
{
    [SerializeField] EasyCharacterMovement.Templates.SideScrollerTemplate.MyCharacter player;

    public void OnTriggerEnter(Collider fall)
    {
        if (fall.CompareTag("Player"))
        {
            player.health = 0;
        }
    }
}
