using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMove : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (transform.localPosition.x <= -9)
        {
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().velocity = new Vector3(3, 0, 0);
        }
        else if (transform.localPosition.x >= 18)
        {
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().velocity = new Vector3(-3, 0, 0);
        }
    }
}
