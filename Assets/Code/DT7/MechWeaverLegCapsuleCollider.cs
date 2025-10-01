using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyCharacterMovement.Templates.SideScrollerTemplate;

public class MechWeaverLegCapsuleCollider : MonoBehaviour
{
    [SerializeField] MechWeaverLeg parentLeg;

    void OnTriggerEnter(Collider collider)
    {

        if (collider.CompareTag("Player"))
        {
            if (collider.GetComponent<MyCharacter>() != null)
            {
                // every time the player is touching a leg, they lose health
                collider.GetComponent<MyCharacter>().health -= 2f;
            }
        }
        else if (collider.CompareTag("BulletPlayer")) // not handed in BulletPlayer since not of type EnemyInterface
        {
            // if MechWeaver is Resetting between phases, legs don't take damage
            if (parentLeg.GetComponentInParent<MechWeaverAgent>().isResetting)
            {
                return;
            }

            parentLeg.health -= 2f; // player will only deal two damage at a time to legs instead of the normal 5
        }
        else
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collider);
            return;
        }
    }

}
