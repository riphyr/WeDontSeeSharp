using System.Collections;
using UnityEngine;

public class CameraLookingAt : MonoBehaviour
{
    [Header("Paramètres d'interaction")]
    public float interactionDistance = 2.5f;
    public GameObject interactionText;

    private Camera playerCamera;
    private PlayerInventory inventory;
    private KeyCode primaryInteractionKey;
	private KeyCode secondaryInteractionKey;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        LoadInteractionKey();
        DetectInteractiveObject();
    }

    private void LoadInteractionKey()
    {
        string primaryKey = PlayerPrefs.GetString("PrimaryInteraction", "None");
        primaryInteractionKey = GetKeyCodeFromString(primaryKey);

		string secondaryKey = PlayerPrefs.GetString("SecondaryInteraction", "None");
        secondaryInteractionKey = GetKeyCodeFromString(secondaryKey);
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

                if (Input.GetKeyDown(primaryInteractionKey)) 
                    door.ToggleDoor();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.LockKey lockKey))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Dévérouiller la porte");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    lockKey.AttemptUnlock();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.PadLock padlock))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Dévérouiller la porte");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    padlock.EnterPadLockMode();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Window window))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, window.IsOpen() ? "Fermer la fenêtre" : "Ouvrir la fenêtre");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    window.ToggleWindow();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Switch lightSwitch))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, lightSwitch.IsOn() ? "Désactiver l'interrupteur" : "Activer l'interrupteur");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    lightSwitch.ToggleSwitch();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Drawer drawer))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, drawer.IsOpen() ? "Fermer le tiroir" : "Ouvrir le tiroir");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    drawer.ToggleDrawer();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Wardrobe wardrobe))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, wardrobe.IsOpen() ? "Fermer l'armoire" : "Ouvrir l'armoire");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    wardrobe.ToggleWardrobe();
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Key key))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Ramasser la clef");
                if (Input.GetKeyDown(primaryInteractionKey))
                    key.PickupKey(inventory);
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Candle candle))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Ramasser la bougie", "Allumer la bougie");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    candle.PickupCandle(inventory);
                else if (Input.GetKeyDown(secondaryInteractionKey))
                    candle.LightCandle(inventory);
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Lighter lighter))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Ramasser le briquet");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    lighter.PickupLighter(inventory);
            }
			else if (hit.transform.TryGetComponent(out InteractionScripts.MatchBox matchBox))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Ramasser les allumettes");
                if (Input.GetKeyDown(primaryInteractionKey)) 
                    matchBox.PickupMatchBox(inventory);
            }
			else if (hit.transform.TryGetComponent(out InteractionScripts.Battery battery))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Ramasser la pile");

                if (Input.GetKeyDown(primaryInteractionKey))
                    battery.PickupBattery(inventory);
            }
            else if (hit.transform.TryGetComponent(out InteractionScripts.Flashlight flashlight))
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.blue);
                ShowInteractionText(true, $"Ramasser la lampe torche");

                if (Input.GetKeyDown(primaryInteractionKey))
                    flashlight.PickupFlashlight(inventory);
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

    private void ShowInteractionText(bool show, string message1 = "", string message2 = null)
    {
        if (interactionText)
        {
            interactionText.SetActive(show);
            if (show)
            {
                if (!string.IsNullOrEmpty(message2))
                {
                    interactionText.GetComponent<TMPro.TextMeshProUGUI>().text = $"{message1} [{primaryInteractionKey}]\n{message2} [{secondaryInteractionKey}]";
                }
                else
                {
                    interactionText.GetComponent<TMPro.TextMeshProUGUI>().text = $"{message1} [{primaryInteractionKey}]";
                }
            }
        }
    }
}
