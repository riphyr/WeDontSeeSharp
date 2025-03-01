using System.Collections;
using UnityEngine;

public class CameraLookingAt : MonoBehaviour
{
    [Header("Paramètres d'interaction")]
    public float interactionDistance = 2.5f;
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

            if (hit.transform.TryGetComponent(out InteractionScripts.Door door))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, door.IsOpen() ? "Fermer la porte" : "Ouvrir la porte");

                if (Input.GetKeyDown(interactKey)) 
                    door.ToggleDoor();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.LockKey lockKey))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Dévérouiller la porte");
                if (Input.GetKeyDown(interactKey)) 
                    lockKey.AttemptUnlock();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.PadLock padlock))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Dévérouiller la porte");
                if (Input.GetKeyDown(interactKey)) 
                    padlock.EnterPadLockMode();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Window window))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, window.IsOpen() ? "Fermer la fenêtre" : "Ouvrir la fenêtre");
                if (Input.GetKeyDown(interactKey)) 
                    window.ToggleWindow();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Switch lightSwitch))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, lightSwitch.IsOn() ? "Désactiver l'interrupteur" : "Activer l'interrupteur");
                if (Input.GetKeyDown(interactKey)) 
                    lightSwitch.ToggleSwitch();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Drawer drawer))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, drawer.IsOpen() ? "Fermer le tiroir" : "Ouvrir le tiroir");
                if (Input.GetKeyDown(interactKey)) 
                    drawer.ToggleDrawer();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Wardrobe wardrobe))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, wardrobe.IsOpen() ? "Fermer l'armoire" : "Ouvrir l'armoire");
                if (Input.GetKeyDown(interactKey)) 
                    wardrobe.ToggleWardrobe();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Key key))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Ramasser la clef");

                if (Input.GetKeyDown(interactKey))
                {
                    PlayerInventory inventory = GetComponent<PlayerInventory>();
                    if (inventory != null)
                    {
                        key.PickupKey(inventory);
                    }
                }
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
