using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            SceneManager.LoadScene("EndLose");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            SceneManager.LoadScene("NewEnd");

        }
    }
}
