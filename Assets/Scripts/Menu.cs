using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    /*private float timeElapsed;

    void Update()
    {
        if(SceneManager.GetActiveScene().name == "Death" || SceneManager.GetActiveScene().name == "Win")
        {
            timeElapsed += Time.deltaTime;
            Debug.Log(timeElapsed);
            if (timeElapsed > 5)
            {
                timeElapsed = 0;
                SceneManager.LoadScene(0);
            }
        }
    }*/

    public void OnPlayButton()
    {
        SceneManager.LoadScene(1);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void OnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
