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
            { typeof(InteractionScripts.Crowbar), hit => HandleCrowbar(hit) },
            { typeof(InteractionScripts.ElectricBox), hit => HandleElectricBox(hit) },
            { typeof(InteractionScripts.ElectricButton), hit => HandleElectricButton(hit) },
            { typeof(InteractionScripts.ElectricLever), hit => HandleElectricLever(hit) },
            { typeof(InteractionScripts.ElectricScrew), hit => HandleElectricScrew(hit) },
            { typeof(InteractionScripts.CDReader), hit => HandleCDReader(hit) },
            { typeof(InteractionScripts.EMFDetector), hit => HandleEMFDetector(hit) },
            { typeof(InteractionScripts.CDDisk), hit => HandleCDDisk(hit) },
            { typeof(InteractionScripts.Magnetophone), hit => HandleMagnetophone(hit) },
            { typeof(InteractionScripts.KeyboardCameraSwitcher), hit => HandleKeyboardCameraSwitcher(hit) },
			{ typeof(InteractionScripts.SafeDial), hit => HandleSafeDial(hit) },
			{ typeof(InteractionScripts.SafeValve), hit => HandleSafeValve(hit) },
			{ typeof(InteractionScripts.SafeDoor), hit => HandleSafeDoor(hit) },
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

            foreach (var entry in interactionHandlers)
			{
    			if (hit.transform.TryGetComponent(entry.Key, out var component))
    			{
        			entry.Value.Invoke(hit);
        			return;
    			}
			}
			ShowInteractionText(false);
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
        ShowInteractionText(true, door.IsOpen() ? "Close the door" : "Open the door");
        if (Input.GetKeyDown(primaryInteractionKey)) door.ToggleDoor();
    }

    private void HandleLockKey(RaycastHit hit)
    {
        ShowInteractionText(true, "Unlock the door");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.LockKey>().AttemptUnlock();
    }

    private void HandlePadLock(RaycastHit hit)
    {
        ShowInteractionText(true, "Unlock the door");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.PadLock>().EnterPadLockMode();
    }

    private void HandleWindow(RaycastHit hit)
    {
        ShowInteractionText(true, hit.transform.GetComponent<InteractionScripts.Window>().IsOpen() ? "Close the window" : "Open the window");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Window>().ToggleWindow();
    }

    private void HandleSwitch(RaycastHit hit)
    {
        ShowInteractionText(true, hit.transform.GetComponent<InteractionScripts.Switch>().IsOn() ? "Turn off the switch" : "Turn on the switch");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Switch>().ToggleSwitch();
    }    

    private void HandleDrawer(RaycastHit hit)
    {
        ShowInteractionText(true, hit.transform.GetComponent<InteractionScripts.Drawer>().IsOpen() ? "Close the drawer" : "Open the drawer");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Drawer>().ToggleDrawer();
    }

    private void HandleWardrobe(RaycastHit hit)
    {
        ShowInteractionText(true, hit.transform.GetComponent<InteractionScripts.Wardrobe>().IsOpen() ? "Close the wardrobe" : "Open the wardrobe");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Wardrobe>().ToggleWardrobe();
    }

    private void HandleKey(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the key");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Key>().PickupKey(inventory);
    }

    private void HandleCandle(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the candle", "Light the candle");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Candle>().PickupCandle(inventory);
        else if (Input.GetKeyDown(secondaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Candle>().LightCandle(inventory);
    }

    private void HandleLighter(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the lighter");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Lighter>().PickupLighter(inventory);
    }

    private void HandleMatchBox(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the matchbox");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.MatchBox>().PickupMatchBox(inventory);
    }

    private void HandleBattery(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the battery");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Battery>().PickupBattery(inventory);
    }

    private void HandleFlashlight(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the flashlight");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Flashlight>().PickupFlashlight(inventory);
    }

    private void HandleUVFlashlight(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the UV flashlight");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.UVFlashlight>().PickupFlashlight(inventory);
    }

    private void HandleElectricBox(RaycastHit hit)
    {
        ShowInteractionText(true, hit.transform.GetComponent<InteractionScripts.ElectricBox>().IsOpen() ? "Close the box" : "Open the box");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.ElectricBox>().ToggleBox();
    }

    private void HandleElectricButton(RaycastHit hit)
    {
        ShowInteractionText(true, "Toggle the button");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.ElectricButton>().ToggleButton();
    }

    private void HandleElectricLever(RaycastHit hit)
    {
        ShowInteractionText(true, "Activate the lever");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.ElectricLever>().TryActivateLever();
    }

    private void HandleElectricScrew(RaycastHit hit)
    {
        if (FindObjectOfType<PlayerUsing>().wrenchScript != null)
        {
            ShowInteractionText(true, "Unscrew the screw");
            if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.ElectricScrew>().TryRemoveScrew();
        }
    }

    private void HandleCDReader(RaycastHit hit)
    {
        if (playerUsing != null && playerUsing.HasCDInHand())
        {
            ShowInteractionText(true, "Insert the CD");
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
            ShowInteractionText(true, "Switch camera");
            if (Input.GetKeyDown(primaryInteractionKey)) keyboardCameraSwitcher.NextCamera();
        }
    }

    private void HandleScreen(RaycastHit hit)
    {
        var screen = hit.transform.GetComponent<InteractionScripts.ScreenInteraction>();
        if (screen.isOn())
        {
            ShowInteractionText(true, "Look at the screen");
            if (Input.GetKeyDown(primaryInteractionKey)) screen.EnterScreenMode();
        }
    }

    private void HandleWrench(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the wrench");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Wrench>().PickupWrench(inventory);
    }
    
    private void HandleCrowbar(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the crowbar");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Crowbar>().PickupCrowbar(inventory);
    }

    private void HandleEMFDetector(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the EMF detector");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.EMFDetector>().PickupEMF(inventory);
    }

    private void HandleCDDisk(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the CD");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.CDDisk>().PickupDisk(inventory);
    }

    private void HandleMagnetophone(RaycastHit hit)
    {
        ShowInteractionText(true, "Pick up the tape recorder");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.Magnetophone>().PickupMagnetophone(inventory);
    }

    private void HandleSafeDial(RaycastHit hit)
    {
        var dial = hit.transform.GetComponent<InteractionScripts.SafeDial>();
        
        ShowInteractionText(true, "Increase", "Decrease");
        if (Input.GetKeyDown(primaryInteractionKey)) dial.RotateDial(1);
        if (Input.GetKeyDown(secondaryInteractionKey)) dial.RotateDial(-1);
    }

    private void HandleSafeValve(RaycastHit hit)
    {
        ShowInteractionText(true, "Turn the valve");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.SafeValve>().TryUnlock();
    }

    private void HandleSafeDoor(RaycastHit hit)
    {
        ShowInteractionText(true, "Open the safe");
        if (Input.GetKeyDown(primaryInteractionKey)) hit.transform.GetComponent<InteractionScripts.SafeDoor>().OpenDoor();
    }
}