using System.Collections;
using UnityEngine;

public class CameraLookingAt : MonoBehaviour
{
    [Header("Paramètres d'interaction")]
    public float interactionDistance = 2f;
    public GameObject interactionText;

    private Camera playerCamera;
    private KeyCode interactKey;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        LoadInteractionKey();
        DetectInteractiveObject();
    }

    private void LoadInteractionKey()
    {
        string key = PlayerPrefs.GetString("Interact", "None");
        interactKey = GetKeyCodeFromString(key);
    }
    
    private KeyCode GetKeyCodeFromString(string key)
    {
        return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
    }

    private void DetectInteractiveObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

            if (hit.transform.TryGetComponent(out InteractionScripts.Switch lightSwitch))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, "Utiliser l'interrupteur");
                if (Input.GetKeyDown(interactKey)) lightSwitch.ToggleSwitch();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Drawer drawer))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, "Ouvrir le tiroir");
                if (Input.GetKeyDown(interactKey)) drawer.ToggleDrawer();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Wardrobe wardrobe))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, "Ouvrir l'armoire");
                if (Input.GetKeyDown(interactKey)) wardrobe.ToggleWardrobe();
            }
            else
            {
                ShowInteractionText(false);
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);

            ShowInteractionText(false);
        }
    }

    private void ShowInteractionText(bool show, string message = "")
    {
        if (interactionText)
        {
            interactionText.SetActive(show);
            if (show)
            {
                interactionText.GetComponent<TMPro.TextMeshProUGUI>().text = $"{message} [{interactKey}]";
            }
        }
    }
}
