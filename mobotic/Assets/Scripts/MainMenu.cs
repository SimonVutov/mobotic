using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject LoadSceneMenu;
    public GameObject optionsMenu;

    public void PlayGame() {
        // scene 1, 2, 3, 4 are the scenes for the game. List those 4 and allow user to click on one to select
        mainMenu.SetActive(false);
        LoadSceneMenu.SetActive(true);
    }
    public void LoadScene1() {
        SceneManager.LoadScene(1);
    }
    public void LoadScene2() {
        SceneManager.LoadScene(2);
    }
    public void LoadScene3() {
        SceneManager.LoadScene(3);
    }
    public void LoadScene4() {
        SceneManager.LoadScene(4);
    }

    public void Options() {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void Back() {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void QuitGame() {
        Debug.Log("Quit");
        Application.Quit();
    }
}
