using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;



public class leaderboardHandler : MonoBehaviour
{
    [SerializeField] GameObject leaderboardsUI;
    [SerializeField] GameObject achievementsUI;

    [SerializeField] GameObject leaderboardTemplate;
    [SerializeField] GameObject leaderboards;

    [SerializeField] ScrollRect scrollLeaderboards;

    //makes user able to interact with space in between the leaderboard objects
    [SerializeField] RectTransform rectScrollRaycast;

    //markers for top and bottom of the scrolling bounds
    [SerializeField] RectTransform rectBottomBound;
    [SerializeField] RectTransform rectTopBound;

    //amount of leaderboards to create
    GameObject[] newLeaderboard = new GameObject[20];


    // Start is called before the first frame update
    void Start()
    {
        populateLeaderboards();
    }

    public void onShowAchievements()
    {
        leaderboardsUI.SetActive(false);
        achievementsUI.SetActive(true);
    }
    //procedurally generating the additional leaderboards for the panel and expanding raycastable area
    public void populateLeaderboards()
    {

        Vector2 handleBottom = rectScrollRaycast.anchorMin;

        for (int i = 0; i < newLeaderboard.Length; i++)
        {
            newLeaderboard[i] = Instantiate(leaderboardTemplate, leaderboards.transform) as GameObject;
            RectTransform rectNewleaderboardTransform = newLeaderboard[i].GetComponent<RectTransform>();

            Vector2 topBound = rectNewleaderboardTransform.anchorMax;
            Vector2 bottomBound = rectNewleaderboardTransform.anchorMin;
            

            topBound.y -= (i + 1) * .1f;
            bottomBound.y -= (i + 1) * .1f;
            handleBottom.y -= .175f;

            rectNewleaderboardTransform.anchorMax = topBound;

            rectNewleaderboardTransform.anchorMin = bottomBound;
            
        }
        
        rectScrollRaycast.anchorMin = handleBottom;
    }

    
    public void onScrollleaderboards()
    {
        
        RectTransform rectBottomLeaderboard = newLeaderboard[newLeaderboard.Length-1].GetComponent<RectTransform>();
        RectTransform rectLeaderboards = leaderboards.GetComponent<RectTransform>();

        float heightRelative = rectTopBound.localPosition.y - rectBottomBound.localPosition.y;

        float Max = Mathf.Abs(rectBottomLeaderboard.localPosition.y + heightRelative);
        float Min = 0.0f;

        float start = rectLeaderboards.localPosition.y;
        
       //test for top and bottom scrolling bounds
        if (start < Min)
        {
            scrollLeaderboards.StopMovement();

            rectLeaderboards.localPosition = new Vector3(rectLeaderboards.localPosition.x, Min, rectLeaderboards.localPosition.z);
            Debug.Log("hit that top");
        }
        else if (start > Max)
        {
            scrollLeaderboards.StopMovement();

            rectLeaderboards.localPosition = new Vector3(rectLeaderboards.localPosition.x, Max, rectLeaderboards.localPosition.z);
            Debug.Log("hit that bottom");
        }
        else
        {
            Debug.Log("were doing greeat");
        }
    }

    public void hideleaderboards()
    {
        leaderboardsUI.SetActive(false);
    }
}
