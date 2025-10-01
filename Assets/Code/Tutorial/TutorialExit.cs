using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialExit : MonoBehaviour
{
    [SerializeField] GameObject mountain;
    [SerializeField] GameObject topRenderWall;
    [SerializeField] GameObject firstStageRenderWall;
    [SerializeField] GameObject stageOneEnemies;
    [SerializeField] GameObject stageOneItems;

    private void OnTriggerEnter(Collider other)
    {
        topRenderWall.SetActive(true);
        mountain.SetActive(false);
        firstStageRenderWall.SetActive(true);
        stageOneEnemies.SetActive(true);
    }
}
