using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Forwarding to the game
    public void OnPlayButton()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(1);
    }

    // Quits the game
    public void OnQuitButton()
    {
        Application.Quit();
    }

    //Forwarding to the menu
    public void OnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
