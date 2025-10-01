/**************************
 *******TENSORCHESS********
 *************************/

using UnityEngine;
using UnityEngine.SceneManagement;


//THIS SCRIPT IS RESPONSIBLE FOR CONTROLLING MENU NAVIGATION OF THE GAME

public class menuController : MonoBehaviour
{

    [SerializeField] GameObject menuMain;
    [SerializeField] GameObject menuNewGame;
    [SerializeField] GameObject menuLoadGame;
    [SerializeField] GameObject menuSettings;
    [SerializeField] GameObject menuAccolades;


    GameObject[] gameMenu;


    // Start is called before the first frame update
    void Start()
    {
        gameMenu = new GameObject[] {menuMain, menuNewGame, menuLoadGame, menuSettings, menuAccolades};
        showMainMenu();
    }

  
    public void showMainMenu()
    {
        display(menuMain);
    }

    public void showNewGame()
    {
        
    }

    public void showLoadGame()
    {
        display(menuLoadGame);
    }

    public void showSettings()
    {
        display(menuSettings);
    }

    public void showAccolades()
    {
        display(menuAccolades);
    }

    public void loadTutorial()
    {
        SceneManager.LoadScene(1);
    }

 
    //Hides all menus to prepare for call of diaplay function
    public void hideAllMenus()
    {
    foreach(GameObject menu in gameMenu)
        {
            menu.SetActive(false);
        }
    }

    //designates the new active display for the program
    public void display(GameObject newPane)
    {
        hideAllMenus();
        newPane.SetActive(true);
    }

}