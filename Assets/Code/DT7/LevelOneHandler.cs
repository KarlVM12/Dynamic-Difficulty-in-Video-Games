using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class LevelOneHandler : MonoBehaviour, LevelHandler
{
    [SerializeField] GameObject stageOneEnemies;
    [SerializeField] GameObject stageTwoEnemies;
    [SerializeField] GameObject stageThreeEnemies;
    [SerializeField] GameObject stageOneItems;
    [SerializeField] GameObject stageTwoItems;
    [SerializeField] GameObject stageThreeItems;

    void LevelHandler.resetLevel()
    {
        if (stageOneEnemies != null && stageOneEnemies.activeSelf)
        { 

            foreach (Transform enemy in stageOneEnemies.transform)
            {
                enemy.gameObject.SetActive(false);
                EnemyInterface enemyInterface = null;
                if ((enemyInterface = enemy.GetComponent<EnemyInterface>()) != null)
                {
                    enemy.gameObject.SetActive(true);

                    enemy.position = enemyInterface.spawn;
                    enemyInterface.health = enemyInterface.maxHealth;
                }
                else //if ((enemyInterface = enemy.GetComponentInChildren<EnemyInterface>()) != null) // for if the EnemyInterface has a parent container
                {
                    GameObject enemyChild = enemy.GetChild(0).gameObject;
                    enemy.gameObject.SetActive(true);
                    enemyChild.SetActive(true);

                    enemyInterface = enemyChild.GetComponent<EnemyInterface>();
                    //enemyChild.transform.position = enemyInterface.spawn; // turret doesn't move, might need to fix in future because this is broken
                    enemyInterface.health = enemyInterface.maxHealth;
                }
                //Debug.Log("Level One Enemy: " + enemyInterface);
            }
        }

        if (stageTwoEnemies != null && stageTwoEnemies.activeSelf)
        {

            foreach (Transform enemy in stageTwoEnemies.transform)
            {
                enemy.gameObject.SetActive(false);
                EnemyInterface enemyInterface = null;
                if ((enemyInterface = enemy.GetComponent<EnemyInterface>()) != null)
                {
                    enemy.gameObject.SetActive(true); 

                    enemy.position = enemyInterface.spawn;
                    enemyInterface.health = enemyInterface.maxHealth;
                }
                else //if ((enemyInterface = enemy.GetComponentInChildren<EnemyInterface>()) != null) // for if the EnemyInterface has a parent container
                {
                    GameObject enemyChild = enemy.GetChild(0).gameObject;
                    enemy.gameObject.SetActive(true);
                    enemyChild.SetActive(true);

                    enemyInterface = enemyChild.GetComponent<EnemyInterface>();
                    //enemyChild.transform.position = enemyInterface.spawn; // turret doesn't move, might need to fix in future because this is broken
                    enemyInterface.health = enemyInterface.maxHealth;
                }
            }
        }

        if (stageThreeEnemies != null && stageThreeEnemies.activeSelf)
        {

            foreach (Transform enemy in stageThreeEnemies.transform)
            {
                enemy.gameObject.SetActive(false);
                EnemyInterface enemyInterface = null;
                if ((enemyInterface = enemy.GetComponent<EnemyInterface>()) != null)
                {
                    enemy.gameObject.SetActive(true);

                    enemy.position = enemyInterface.spawn;
                    enemyInterface.health = enemyInterface.maxHealth;
                }
                else //if ((enemyInterface = enemy.GetComponentInChildren<EnemyInterface>()) != null) // for if the EnemyInterface has a parent container
                {
                    GameObject enemyChild = enemy.GetChild(0).gameObject;
                    enemy.gameObject.SetActive(true);
                    enemyChild.SetActive(true);

                    enemyInterface = enemyChild.GetComponent<EnemyInterface>();
                    //enemyChild.transform.position = enemyInterface.spawn; // turret doesn't move, might need to fix in future because this is broken
                    enemyInterface.health = enemyInterface.maxHealth;
                }
            }
        }

        if (stageOneItems != null && stageOneItems.activeSelf)
        {

            foreach (Transform item in stageOneItems.transform)
            {
                item.gameObject.SetActive(true);
  
            }
        }

        if (stageTwoItems != null && stageTwoItems.activeSelf)
        {

            foreach (Transform item in stageTwoItems.transform)
            {
                item.gameObject.SetActive(true);

            }
        }

        if (stageThreeItems != null && stageThreeItems.activeSelf)
        {

            foreach (Transform item in stageThreeItems.transform)
            {
                item.gameObject.SetActive(true);

            }
        }
    }

}
