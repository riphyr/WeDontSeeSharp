using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerUsing : MonoBehaviourPun
{
    [Header("Prefabs")]
    public GameObject matchPrefab;
    public GameObject candlePrefab;
    public GameObject flashlightPrefab;
    public GameObject UVFlashlightPrefab;
    public GameObject wrenchPrefab;
    public GameObject magnetophonePrefab;
    public GameObject emfPrefab;
    public GameObject diskPrefab;

    [Header("Player")]
    private PlayerInventory inventory;
    private CameraLookingAt cameraLookingAt;
    private Camera playerCamera;
    private Transform playerBody;
    private KeyCode nextKeyInteraction;
    private KeyCode previousKeyInteraction;
	private KeyCode useKey;
	private KeyCode reloadKey;
	private bool useCooldown = false;

	[Header("Match")]
	private InteractionScripts.Match matchScript;
    
    [Header("Candle")]
    private GameObject previewCandle;
    private PhotonView candleView;
    private bool placingCandle = false;
    private BoxCollider candleCollider;

    [Header("Flashlight")] 
    private InteractionScripts.Flashlight flashlightScript;
    private CapsuleCollider flashlightCollider;
    
    [Header("UV Flashlight")] 
    private InteractionScripts.UVFlashlight UVFlashlightScript;
    private BoxCollider UVFlashlightCollider;
    
    [Header("Wrench")] 
    [HideInInspector]public InteractionScripts.Wrench wrenchScript;
    private BoxCollider wrenchCollider;
    
    [Header("Magnetophone")]
    private GameObject activeMagnetophone;
    private InteractionScripts.Magnetophone magnetophoneScript;
    private BoxCollider magnetophoneCollider;
    
    [Header("EMF Detector")]
    private InteractionScripts.EMFDetector emfScript;
    private BoxCollider emfCollider;
    
    [Header("Wrench")] 
    [HideInInspector]public InteractionScripts.CDDisk diskScript;
    private BoxCollider diskCollider;
    
    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        playerBody = transform;
        playerCamera = GetComponentInChildren<Camera>();
        cameraLookingAt = GetComponent<CameraLookingAt>();

		LoadInteractionKey();
    }
    
    private KeyCode GetKeyCodeFromString(string key)
    {
        return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
    }
    
    private void LoadInteractionKey()
    {
        string nextKey = PlayerPrefs.GetString("Next", "None");
        nextKeyInteraction = GetKeyCodeFromString(nextKey);

        string previousKey = PlayerPrefs.GetString("Previous", "None");
        previousKeyInteraction = GetKeyCodeFromString(previousKey);

		string useString = PlayerPrefs.GetString("Use", "None");
		useKey = GetKeyCodeFromString(useString);

		string reloadString = PlayerPrefs.GetString("Reload", "None");
		reloadKey = GetKeyCodeFromString(reloadString);
    }
    
    void Update()
    {
        LoadInteractionKey();
		UpdateItemPlacement();
        
		if (useCooldown) 
			return;

        if (Input.GetKeyDown(nextKeyInteraction))
        {
            FindObjectOfType<PlayerInventory>().SwitchToNextItem();
        }
		else if (Input.GetKeyDown(previousKeyInteraction))
        {
            FindObjectOfType<PlayerInventory>().SwitchToPreviousItem();
        }
        else if (Input.GetKeyDown(useKey))
        {
            UseSelectedItem();
        }
		else if (Input.GetKeyDown(reloadKey))
        {
            ReloadSelectedItem();
        }
    }

	private void UseSelectedItem()
    {
        string selectedItem = inventory.GetSelectedItem();

        if (string.IsNullOrEmpty(selectedItem))
        {
            Debug.Log("Aucun objet sélectionné !");
            return;
        }

        Debug.Log($"Utilisation de l'objet : {selectedItem}");

        switch (selectedItem)
        {
            case "Match":
                photonView.RPC("UseMatch", RpcTarget.All);
                break;
            case "Candle":
                TryToPlaceCandle();
                break;
            case "Flashlight":
                photonView.RPC("UseFlashlight", RpcTarget.All);
                break;
            case "UVFlashlight":
                photonView.RPC("UseUVFlashlight", RpcTarget.All);
                break;
            case "Wrench":
                photonView.RPC("UseWrench", RpcTarget.All);
                break;
            case "Magnetophone":
                photonView.RPC("UseMagnetophone", RpcTarget.All);
                break;
            case "EMFDetector":
                photonView.RPC("UseEMFDetector", RpcTarget.All);
                break;
            case "CDDisk":
                photonView.RPC("UseDisk", RpcTarget.All);
                break;
            default:
                Debug.Log($"Aucune action définie pour l'objet {selectedItem}");
                break;
        }
    }

	private void ReloadSelectedItem()
    {
        string selectedItem = inventory.GetSelectedItem();

        if (string.IsNullOrEmpty(selectedItem))
        {
            Debug.Log("Aucun objet sélectionné !");
            return;
        }

        Debug.Log($"Rechargement de l'objet : {selectedItem}");

        switch (selectedItem)
        {
            case "Flashlight":
                RechargeFlashlight();
                break;
            case "UVFlashlight":
                RechargeUVFlashlight();
                break;
            default:
                Debug.Log($"Aucune rechargement définie pour l'objet {selectedItem}");
                break;
        }
    }

	private void UpdateItemPlacement()
	{
		if (placingCandle)
        {
            UpdateCandlePosition();

            if (Input.GetKeyDown(useKey))
            {
                ConfirmCandlePlacement();
            }
        }
	}

	private IEnumerator UseCooldown()
	{
    	useCooldown = true;
    	yield return new WaitForSeconds(0.2f);
    	useCooldown = false;
	}

    // Gestion allumettes
    [PunRPC]
    private void UseMatch()
    {
        if (!photonView.IsMine) 
            return;

        if (matchScript != null || !inventory.HasItem("Match"))
        {
            return;
        }

        inventory.RemoveItem("Match", 1);

        StartCoroutine(SpawnMatch());
    }

    private IEnumerator SpawnMatch()
    {
        Quaternion matchRotation = Quaternion.Euler(-90f, playerBody.eulerAngles.y, 0f);

        GameObject matchInstance = PhotonNetwork.Instantiate(matchPrefab.name, Vector3.zero, matchRotation);

        yield return new WaitForSeconds(0.1f);

        matchScript = matchInstance.GetComponent<InteractionScripts.Match>();

        matchScript.AssignOwner(photonView.Owner, playerBody);
        matchScript.IgniteMatch();
    }
    
    //Gestion bougies
    private void TryToPlaceCandle()
    {
        if (inventory.HasItem("Candle"))
        {
            cameraLookingAt.enabled = false;
            placingCandle = true;
            previewCandle = PhotonNetwork.Instantiate(candlePrefab.name, transform.position, Quaternion.identity);
            candleCollider = previewCandle.GetComponent<BoxCollider>();
            candleCollider.enabled = false;
            
            candleView = previewCandle.GetComponent<PhotonView>();
            if (candleView != null)
            {
                candleView.RequestOwnership(); 
                photonView.RPC("AssignOwner_RPC", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }
    
    [PunRPC]
    private void AssignOwner_RPC(int newOwnerID)
    {
        if (candleView != null)
        {
            Photon.Realtime.Player newOwner = PhotonNetwork.CurrentRoom.GetPlayer(newOwnerID);
            if (newOwner != null)
            {
                candleView.TransferOwnership(newOwner);
            }
        }
    }

    private void UpdateCandlePosition()
    {
        if (candleView != null && !candleView.IsMine)
            return; //
        
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;
        
        Debug.DrawRay(ray.origin, ray.direction * 3, Color.yellow);
        
        if (Physics.Raycast(ray, out hit, 3f))
        {
            Vector3 newPosition = hit.point + new Vector3(-0.05f, 0.125f, -0.05f);
            previewCandle.transform.position = newPosition;
            photonView.RPC("UpdateCandlePosition_RPC", RpcTarget.Others, newPosition);
        }
    }

    [PunRPC]
    private void UpdateCandlePosition_RPC(Vector3 newPosition)
    {
        if (previewCandle != null)
        {
            previewCandle.transform.position = newPosition;
        }
    }
    
    private void ConfirmCandlePlacement()
    {
        placingCandle = false;
        inventory.RemoveItem("Candle", 1);
        cameraLookingAt.enabled = true;
        candleCollider.enabled = true;
		
		StartCoroutine(UseCooldown());
    }
    
    //Gestion lampe torche
    [PunRPC]
    private void UseFlashlight()
    {
        if (!photonView.IsMine) 
            return;
        
        if (flashlightScript == null && inventory.GetItemCount("Flashlight") >= 0)
        {
            StartCoroutine(SpawnFlashlight());
        }
        else if (flashlightScript != null)
        {
            flashlightScript.UnequipFlashlight(inventory);
            flashlightScript = null;
        }
    }
    
    private IEnumerator SpawnFlashlight()
    {
        Quaternion flashlightRotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);

        GameObject flashlightInstance = PhotonNetwork.Instantiate(flashlightPrefab.name, Vector3.zero, flashlightRotation);
        flashlightCollider = flashlightInstance.GetComponent<CapsuleCollider>();
        flashlightCollider.enabled = false;

        yield return new WaitForSeconds(0.1f);

        flashlightScript = flashlightInstance.GetComponent<InteractionScripts.Flashlight>();

        flashlightScript.AssignOwner(photonView.Owner, playerBody);
        flashlightScript.EquipFlashlight(inventory, playerBody);
    }

    private void RechargeFlashlight()
    {
        if (flashlightScript != null && flashlightScript.isOutOfBattery())
        {
            flashlightScript.RechargeBattery(inventory);
        }
    }
    
    //Gestion lampe UV
    [PunRPC]
    private void UseUVFlashlight()
    {
        if (!photonView.IsMine) 
            return;
        
        if (UVFlashlightScript == null && inventory.GetItemCount("UVFlashlight") >= 0)
        {
            StartCoroutine(SpawnUVFlashlight());
        }
        else if (UVFlashlightScript != null)
        {
            UVFlashlightScript.UnequipFlashlight(inventory);
            UVFlashlightScript = null;
        }
    }
    
    private IEnumerator SpawnUVFlashlight()
    {
        Quaternion UVFlashlightRotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);

        GameObject UVFlashlightInstance = PhotonNetwork.Instantiate(UVFlashlightPrefab.name, Vector3.zero, UVFlashlightRotation);
        UVFlashlightCollider = UVFlashlightInstance.GetComponent<BoxCollider>();
        UVFlashlightCollider.enabled = false;

        yield return new WaitForSeconds(0.1f);

        UVFlashlightScript = UVFlashlightInstance.GetComponent<InteractionScripts.UVFlashlight>();

        UVFlashlightScript.AssignOwner(photonView.Owner, playerBody);
        UVFlashlightScript.EquipFlashlight(inventory, playerBody);
    }

    private void RechargeUVFlashlight()
    {
        if (UVFlashlightScript != null && UVFlashlightScript.isOutOfBattery())
        {
            UVFlashlightScript.RechargeBattery(inventory);
        }
    }
    
    // Gestion clef (outils)
    [PunRPC]
    private void UseWrench()
    {
        if (!photonView.IsMine) 
            return;
        
        if (wrenchScript == null && inventory.HasItem("Wrench"))
        {
            StartCoroutine(SpawnWrench());
        }
        else if (wrenchScript != null)
        {
            wrenchScript.ShowWrench(false);
            wrenchScript = null;
        }
    }

    private IEnumerator SpawnWrench()
    {
        Quaternion wrenchRotation = Quaternion.Euler(-90f, playerBody.eulerAngles.y, 0f);

        GameObject wrenchInstance = PhotonNetwork.Instantiate(wrenchPrefab.name, Vector3.zero, wrenchRotation);
        wrenchCollider = wrenchInstance.GetComponent<BoxCollider>();
        wrenchCollider.enabled = false;

        yield return new WaitForSeconds(0.1f);

        wrenchScript = wrenchInstance.GetComponent<InteractionScripts.Wrench>();

        wrenchScript.AssignOwner(photonView.Owner, playerBody);
        wrenchScript.ShowWrench(true);
    }
    
    // Gestion du magnétophone
    [PunRPC]
    private void UseMagnetophone()
    {
        if (!photonView.IsMine) 
            return;

        if (magnetophoneScript == null && inventory.HasItem("Magnetophone"))
        {
            StartCoroutine(SpawnMagnetophone());
        }
        else if (magnetophoneScript != null)
        {
            magnetophoneScript.ShowMagnetophone(false);
            magnetophoneScript = null;
        }
    }

    private IEnumerator SpawnMagnetophone()
    {
        Quaternion magnetophoneRotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);
        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;

        GameObject magnetophoneInstance = PhotonNetwork.Instantiate(magnetophonePrefab.name, spawnPosition, magnetophoneRotation);
        magnetophoneCollider = magnetophoneInstance.GetComponent<BoxCollider>();
        magnetophoneCollider.enabled = false;

        yield return new WaitForSeconds(0.1f);

        magnetophoneScript = magnetophoneInstance.GetComponent<InteractionScripts.Magnetophone>();

        if (magnetophoneScript != null)
        {
            magnetophoneScript.AssignOwner(photonView.Owner, playerBody);
            magnetophoneScript.ActivateMagnetophone();
        }
    }
    
    // Gestion de l'EMF
    [PunRPC]
    private void UseEMFDetector()
    {
        if (!photonView.IsMine) 
            return;

        if (emfScript == null && inventory.HasItem("EMFDetector"))
        {
            StartCoroutine(SpawnEMFDetector());
        }
        else if (emfScript != null)
        {
            emfScript.ToggleEMF();
        }
    }

    private IEnumerator SpawnEMFDetector()
    {
        Quaternion emfRotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);
        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;

        GameObject emfInstance = PhotonNetwork.Instantiate(emfPrefab.name, spawnPosition, emfRotation);
        emfCollider = emfInstance.GetComponent<BoxCollider>();
        emfCollider.enabled = false;

        yield return new WaitForSeconds(0.1f);

        emfScript = emfInstance.GetComponent<InteractionScripts.EMFDetector>();

        if (emfScript != null)
        {
            emfScript.AssignOwner(photonView.Owner, playerBody);
            emfScript.ToggleEMF();
        }
    }
    
    // Gestion CD Disk
    [PunRPC]
    private void UseDisk()
    {
        if (!photonView.IsMine) 
            return;
        
        if (diskScript == null && inventory.HasItem("CDDisk"))
        {
            StartCoroutine(SpawnDisk());
        }
        else if (diskScript != null)
        {
            diskScript.ShowDisk(false);
            diskScript = null;
        }
    }

    private IEnumerator SpawnDisk()
    {
        Quaternion diskRotation = Quaternion.Euler(-90f, playerBody.eulerAngles.y, 0f);

        GameObject diskInstance = PhotonNetwork.Instantiate(diskPrefab.name, Vector3.zero, diskRotation);
        diskCollider = diskInstance.GetComponent<BoxCollider>();
        diskCollider.enabled = false;

        yield return new WaitForSeconds(0.1f);

        diskScript = diskInstance.GetComponent<InteractionScripts.CDDisk>();

        diskScript.AssignOwner(photonView.Owner, playerBody);
        diskScript.ShowDisk(true);
    }
    
    public bool HasCDInHand()
    {
        return diskScript != null && diskScript.isTaken;
    }
}