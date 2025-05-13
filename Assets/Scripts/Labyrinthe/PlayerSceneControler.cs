using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneController : MonoBehaviour
{
    public MonoBehaviour[] extraScripts;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "labyrinthe")
        {
            foreach (var script in extraScripts)
                script.enabled = true;
        }
        else
        {
            foreach (var script in extraScripts)
                script.enabled = false;
        }
    }
}

