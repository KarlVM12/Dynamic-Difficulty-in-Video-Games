using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class doorTrigger : MonoBehaviour
{
    public UnityEvent doorAnimationOpen;
    public UnityEvent doorAnimationClosed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            doorAnimationOpen.Invoke();
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorAnimationClosed.Invoke();

        }

    }

}
