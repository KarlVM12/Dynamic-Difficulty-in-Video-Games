using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;



public class achievementHandler : MonoBehaviour
{
    [SerializeField] GameObject achievementsUI;
    [SerializeField] GameObject leaderboardsUI;

    [SerializeField] GameObject achievementTemplate;
    [SerializeField] GameObject achievements;

    [SerializeField] ScrollRect scrollAchievements;

    //makes user able to interact with space in between the achievement objects
    [SerializeField] RectTransform rectScrollRaycast;

    //markers for top and bottom of the scrolling bounds
    [SerializeField] RectTransform rectBottomBound;
    [SerializeField] RectTransform rectTopBound;

    //amount of achievements to create
    GameObject[] newAchievement = new GameObject[5];




    // Start is called before the first frame update
    void Start()
    {
        populateAchievements();
    }

    public void onShowLeadboards()
    {
        leaderboardsUI.SetActive(true);
        achievementsUI.SetActive(false);
    }
    //procedurally generating the additional achievements for the panel and expanding raycastable area
    public void populateAchievements()
    {

        Vector2 handleBottom = rectScrollRaycast.anchorMin;

        for (int i = 0; i < newAchievement.Length; i++)
        {
            newAchievement[i] = Instantiate(achievementTemplate, achievements.transform) as GameObject;
            RectTransform rectNewAchievementTransform = newAchievement[i].GetComponent<RectTransform>();

            Vector2 topBound = rectNewAchievementTransform.anchorMax;
            Vector2 bottomBound = rectNewAchievementTransform.anchorMin;
            

            topBound.y -= (i + 1) * .2f;
            bottomBound.y -= (i + 1) * .2f;
            handleBottom.y -= .175f;

            rectNewAchievementTransform.anchorMax = topBound;

            rectNewAchievementTransform.anchorMin = bottomBound;
            
        }
        
        rectScrollRaycast.anchorMin = handleBottom;
    }

    
    public void onScrollAchievements()
    {
        
        RectTransform rectBottomAchievement = newAchievement[newAchievement.Length-1].GetComponent<RectTransform>();
        RectTransform rectAchievements = achievements.GetComponent<RectTransform>();

        float heightRelative = rectTopBound.localPosition.y - rectBottomBound.localPosition.y;

        float Max = Mathf.Abs(rectBottomAchievement.localPosition.y + heightRelative);
        float Min = 0.0f;

        float start = rectAchievements.localPosition.y;
        
       //test for top and bottom scrolling bounds
        if (start < Min)
        {
            scrollAchievements.StopMovement();

            rectAchievements.localPosition = new Vector3(rectAchievements.localPosition.x, Min, rectAchievements.localPosition.z);
            Debug.Log("hit that top");
        }
        else if (start > Max)
        {
            scrollAchievements.StopMovement();

            rectAchievements.localPosition = new Vector3(rectAchievements.localPosition.x, Max, rectAchievements.localPosition.z);
            Debug.Log("hit that bottom");
        }
        else
        {
            Debug.Log("were doing greeat");
        }
    }

    public void hideAchievements()
    {
        achievementsUI.SetActive(false);
    }
}
