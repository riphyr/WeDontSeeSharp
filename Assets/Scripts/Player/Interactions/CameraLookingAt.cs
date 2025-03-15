using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraLookingAt : MonoBehaviour
{
    [Header("Paramètres d'interaction")]
    public float interactionDistance = 2.5f;
    public GameObject interactionText;

    private Camera playerCamera;
    private PlayerInventory inventory;
	private PlayerUsing playerUsing;
    private KeyCode primaryInteractionKey;
    private KeyCode secondaryInteractionKey;

    private Dictionary<Type, Action<RaycastHit>> interactionHandlers;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        playerCamera = GetComponentInChildren<Camera>();
		playerUsing = GetComponent<PlayerUsing>();

        LoadInteractionKey();

        interactionHandlers = new Dictionary<Type, Action<RaycastHit>>()
        {
            { typeof(InteractionScripts.Door), hit => HandleDoor(hit) },
            { typeof(InteractionScripts.LockKey), hit => HandleLockKey(hit) },
            { typeof(InteractionScripts.PadLock), hit => HandlePadLock(hit) },
            { typeof(InteractionScripts.Window), hit => HandleWindow(hit) },
            { typeof(InteractionScripts.Switch), hit => HandleSwitch(hit) },
            { typeof(InteractionScripts.Drawer), hit => HandleDrawer(hit) },
            { typeof(InteractionScripts.Wardrobe), hit => HandleWardrobe(hit) },
            { typeof(InteractionScripts.Key), hit => HandleKey(hit) },
            { typeof(InteractionScripts.Candle), hit => HandleCandle(hit) },
            { typeof(InteractionScripts.Lighter), hit => HandleLighter(hit) },
            { typeof(InteractionScripts.MatchBox), hit => HandleMatchBox(hit) },
            { typeof(InteractionScripts.Battery), hit => HandleBattery(hit) },
            { typeof(InteractionScripts.Flashlight), hit => HandleFlashlight(hit) },
            { typeof(InteractionScripts.UVFlashlight), hit => HandleUVFlashlight(hit) },
            { typeof(InteractionScripts.Wrench), hit => HandleWrench(hit) },
            { typeof(InteractionScripts.ElectricBox), hit => HandleElectricBox(hit) },
            { typeof(InteractionScripts.ElectricButton), hit => HandleElectricButton(hit) },
            { typeof(InteractionScripts.ElectricLever), hit => HandleElectricLever(hit) },
            { typeof(InteractionScripts.ElectricScrew), hit => HandleElectricScrew(hit) },
            { typeof(InteractionScripts.CDReader), hit => HandleCDReader(hit) },
            { typeof(InteractionScripts.EMFDetector), hit => HandleEMFDetector(hit) },
            { typeof(InteractionScripts.CDDisk), hit => HandleCDDisk(hit) },
            { typeof(InteractionScripts.Magnetophone), hit => HandleMagnetophone(hit) },
            { typeof(InteractionScripts.KeyboardCameraSwitcher), hit => HandleKeyboardCameraSwitcher(hit) },
            { typeof(InteractionScripts.ScreenInteraction), hit => HandleScreen(hit) }
        };
    }

    void Update()
    {
        LoadInteractionKey();
        DetectInteractiveObject();
    }

    private void LoadInteractionKey()
    {
        primaryInteractionKey = GetKeyCodeFromString(PlayerPrefs.GetString("PrimaryInteraction", "None"));
        secondaryInteractionKey = GetKeyCodeFromString(PlayerPrefs.GetString("SecondaryInteraction", "None"));
    }

    private KeyCode GetKeyCodeFromString(string key)
    {
        return (KeyCode)Enum.Parse(typeof(KeyCode), key);
    }

    private void DetectInteractiveObject()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null) return;
        }

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

            Type objectType = hit.transform.GetComponent<Component>().GetType();

            if (interactionHandlers.TryGetValue(objectType, out Action<RaycastHit> handler))
            {
                handler.Invoke(hit);
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
                interactionText.GetComponent<TextMeshProUGUI>().text = !string.IsNullOrEmpty(message2) 
                    ? $"{message1} [{primaryInteractionKey}]\n{message2} [{secondaryInteractionKey}]"
                    : $"{message1} [{primaryInteractionKey}]";
            }
        }
    }

    private void HandleDoor(RaycastHit hit)
    {
        var door = hit.transform.GetComponent<InteractionScripts.Door>();
        ShowInteractionText(true, door.IsOpen() ? "Fermer la porte" : "Ouvrir la porte");
        if (Input.GetKeyDown(primaryInteractionKey)) door.ToggleDoor();
    }

    private void HandleLockKey(RaycastHit hit)
    {
        ShowInteractionText(true, "Déverrouiller la porte");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.LockKey>().AttemptUnlock();
    }

    private void HandlePadLock(RaycastHit hit)
    {
        ShowInteractionText(true, "Déverrouiller la porte");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.PadLock>().EnterPadLockMode();
    }

    private void HandleCDReader(RaycastHit hit)
    {
		if (playerUsing != null && playerUsing.HasCDInHand())
        {
            ShowInteractionText(true, "Insérer le CD");
            if (Input.GetKeyDown(primaryInteractionKey))
            {
                inventory.RemoveItem("CDDisk");
                playerUsing.diskScript.TryInsertIntoReader(hit.transform.GetComponent<InteractionScripts.CDReader>());
            }
        }
    }

    private void HandleKeyboardCameraSwitcher(RaycastHit hit)
    {
        var keyboardCameraSwitcher = hit.transform.GetComponent<InteractionScripts.KeyboardCameraSwitcher>();
        if (keyboardCameraSwitcher.isOn())
        {
            ShowInteractionText(true, "Changer de caméra");
            if (Input.GetKeyDown(primaryInteractionKey)) keyboardCameraSwitcher.NextCamera();
        }
    }

    private void HandleScreen(RaycastHit hit)
    {
        var screen = hit.transform.GetComponent<InteractionScripts.ScreenInteraction>();
        if (screen.isOn())
        {
            ShowInteractionText(true, "Regarder l'écran");
            if (Input.GetKeyDown(primaryInteractionKey)) screen.EnterScreenMode();
        }
    }

    private void HandleWrench(RaycastHit hit)
    {
        ShowInteractionText(true, "Ramasser la clef à boulon");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Wrench>().PickupWrench(inventory);
    }

    private void HandleEMFDetector(RaycastHit hit)
    {
        ShowInteractionText(true, "Ramasser le détecteur EMF");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.EMFDetector>().PickupEMF(inventory);
    }

    private void HandleCDDisk(RaycastHit hit)
    {
        ShowInteractionText(true, "Ramasser le CD");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.CDDisk>().PickupDisk(inventory);
    }

    private void HandleMagnetophone(RaycastHit hit)
    {
        ShowInteractionText(true, "Ramasser le magnétophone");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Magnetophone>().PickupMagnetophone(inventory);
    }
}