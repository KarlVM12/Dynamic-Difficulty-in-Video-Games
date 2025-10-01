using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class sceneComplete : MonoBehaviour
{

    public Boolean complete = false;

    [SerializeField] UnityEngine.UI.Image fadeToBlack;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (complete)
        {
            Color fading = fadeToBlack.color;

            if (fading.a <= 1.0f)
            {
                fading.a += 1f * Time.deltaTime;
                fadeToBlack.color = fading;
            }
            else
            {
                complete = false;
            }
        }
    }

    private void OnTriggerEnter(Collider end)
    {
        if (end.CompareTag("Player"))
        {
            complete=true;
            StartCoroutine(changeScene());
        }
    }

    IEnumerator changeScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("levelOneBoss");
        
    }
}
