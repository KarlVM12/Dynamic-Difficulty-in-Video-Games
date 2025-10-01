using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class exitDemo : MonoBehaviour
{
    
    public void demoComplete()
    {
        SceneManager.LoadScene("menu");
    }
}
