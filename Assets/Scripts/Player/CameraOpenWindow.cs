using System.Collections;
using UnityEngine;

public class CameraOpenWindow : MonoBehaviour
{
    private Camera playerCamera;
    public float interactionDistance = 2f;
    private KeyCode interactionKey;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.transform.GetComponentInParent<WindowScript.Window>())
            {
                string interactKey = PlayerPrefs.GetString("Interact", "None");
                if (Input.GetKeyDown(GetKeyCodeFromString(interactKey)))
                    hit.transform.GetComponentInParent<WindowScript.Window>().ToggleWindow();
            }
        }
    }
    
    private KeyCode GetKeyCodeFromString(string key)
    {
        return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
    }
}