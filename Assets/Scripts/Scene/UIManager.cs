using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public SceneFader sceneFader;

    //Destroys the Level UI by load
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        //DontDestroyOnLoad(gameObject);
    }

    //Sets the Level UI
    private void Start()
    {
        sceneFader = GetComponentInChildren<SceneFader>();
    }
}
