using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class loadingScreen : MonoBehaviour
{

    public ProgressBarCircle loadProgress;
    public void loadLevel(string levelToLoad)
    {
        StartCoroutine(LoadLevelASync(levelToLoad));
    }

    private IEnumerator LoadLevelASync(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        while (!loadOperation.isDone)
        {

            loadProgress.BarValue = Mathf.Round((loadOperation.progress / .9f) * 100f);
            Debug.Log(loadOperation.progress);
            
            yield return null;
        }
    }
}
