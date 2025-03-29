using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class PlayerUsing : MonoBehaviourPun
{
    [Header("Prefabs")]
    public GameObject matchPrefab;
	public GameObject matchBoxPrefab;
    public GameObject candlePrefab;
    public GameObject flashlightPrefab;
    public GameObject UVFlashlightPrefab;
    public GameObject wrenchPrefab;
    public GameObject magnetophonePrefab;
    public GameObject emfPrefab;
    public GameObject diskPrefab;
	public GameObject redKeyPrefab;
    public GameObject blueKeyPrefab;
    public GameObject greenKeyPrefab;
    public GameObject blackKeyPrefab;
	public GameObject lighterPrefab;
	public GameObject batteryPrefab;
    public GameObject crowbarPrefab;

    [Header("Player")]
    private PlayerInventory inventory;
    private CameraLookingAt cameraLookingAt;
    private Camera playerCamera;
    private Transform playerBody;
    private KeyCode nextKeyInteraction;
    private KeyCode previousKeyInteraction;
	private KeyCode useKey;
	private KeyCode reloadKey;
	private KeyCode dropKey;
	private bool useCooldown = false;
    private Dictionary<string, bool> equippedItems = new Dictionary<string, bool>();

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
    
    [Header("CD Disk")] 
    [HideInInspector]public InteractionScripts.CDDisk diskScript;
    private BoxCollider diskCollider;
    
    [Header("Crowbar")] 
    [HideInInspector]public InteractionScripts.Crowbar crowbarScript;
    private BoxCollider crowbarCollider;
    
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
        string nextKey = PlayerPrefs.GetString("Next", "RightArrow");
        nextKeyInteraction = GetKeyCodeFromString(nextKey);
    
        string previousKey = PlayerPrefs.GetString("Previous", "LeftArrow");
        previousKeyInteraction = GetKeyCodeFromString(previousKey);
    
        string useString = PlayerPrefs.GetString("Use", "A");
        useKey = GetKeyCodeFromString(useString);
    
        string reloadString = PlayerPrefs.GetString("Reload", "R");
        reloadKey = GetKeyCodeFromString(reloadString);
    
        string dropString = PlayerPrefs.GetString("Drop", "T");
        dropKey = GetKeyCodeFromString(dropString);
    }
    
    void Update()
    {
        LoadInteractionKey();
		UpdateItemPlacement();
        
		if (useCooldown) 
			return;

        if (Input.GetKeyDown(nextKeyInteraction))
        {
            Debug.Log("[DEBUG] Touche Next pressée !");
            inventory.SwitchToNextItem();
        }
        else if (Input.GetKeyDown(previousKeyInteraction))
        {
            Debug.Log("[DEBUG] Touche Previous pressée !");
            inventory.SwitchToPreviousItem();
        }
        else if (Input.GetKeyDown(useKey))
        {
            UseSelectedItem();
        }
		else if (Input.GetKeyDown(reloadKey))
        {
            ReloadSelectedItem();
        }
		else if (Input.GetKeyDown(dropKey))
        {
            DropSelectedItem();
        }
    }

    private void UseSelectedItem()
    {
        string selectedItem = inventory.GetSelectedItem();

        if (string.IsNullOrEmpty(selectedItem))
        {
            Debug.LogWarning("[DropSelectedItem] Aucun objet sélectionné ! Assure-toi que 'selectedItemIndex' est bien défini.");
            return;
        }

        Debug.Log($"Utilisation de l'objet : {selectedItem}");

        if (equippedItems.ContainsKey(selectedItem))
        {
            equippedItems[selectedItem] = !equippedItems[selectedItem];
        }
        else
        {
            equippedItems[selectedItem] = true;
        }

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
            case "Crowbar":
                photonView.RPC("UseCrowbar", RpcTarget.All);
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

        inventory.UpdateActionText();
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

	private void DropSelectedItem(bool stack = false, string selectedItem = "")
    {
		if (string.IsNullOrEmpty(selectedItem))
        {
            selectedItem = inventory.GetSelectedItem();
        }

        GameObject itemPrefab = selectedItem switch
        {
            "Flashlight" => flashlightPrefab,
            "UVFlashlight" => UVFlashlightPrefab,
            "CDDisk" => diskPrefab,
            "Lighter" => lighterPrefab,
            "RedKey" => redKeyPrefab,
            "BlueKey" => blueKeyPrefab,
            "GreenKey" => greenKeyPrefab,
            "BlackKey" => blackKeyPrefab,
            "Wrench" => wrenchPrefab,
            "EMFDetector" => emfPrefab,
            "Magnetophone" => magnetophonePrefab,
            "Battery" => batteryPrefab,
            "Crowbar" => crowbarPrefab,
			"Match" => matchBoxPrefab,
			"Candle" => candlePrefab,
            _ => null
        };

        if (itemPrefab == null)
        {
            Debug.Log($"Aucun prefab associé pour {selectedItem}");
            return;
        }

        ForceUnequipItem(selectedItem);

        Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * 0.75f + Vector3.up * 0.2f;
        Quaternion dropRotation = Quaternion.identity;
        GameObject droppedItem = null;

		float amount = 1f;

        if (stack || selectedItem == "Flashlight" || selectedItem == "UVFlashlight")
            amount = inventory.GetItemCount(selectedItem);
        else if (selectedItem == "Battery")
            amount = 100f;

        if (amount <= 0f)
            return;

        // Cas particuliers avec batterie (retirer AVANT le spawn pour transmettre la bonne valeur)
        if (selectedItem == "Flashlight" || selectedItem == "UVFlashlight")
        {
            object[] instantiationData = new object[] { amount };
            inventory.RemoveItem(selectedItem, amount);

            droppedItem = PhotonNetwork.Instantiate(itemPrefab.name, dropPosition, dropRotation, 0, instantiationData);

            if (selectedItem == "Flashlight" && droppedItem.TryGetComponent(out InteractionScripts.Flashlight flashlight))
            {
                flashlight.AssignOwner(photonView.Owner, playerBody);
                flashlight.SetCurrentBattery(amount);
                droppedItem.GetComponent<PhotonView>().RPC("SyncBattery", RpcTarget.AllBuffered, amount);
            }
            else if (selectedItem == "UVFlashlight" && droppedItem.TryGetComponent(out InteractionScripts.UVFlashlight uvFlashlight))
            {
                uvFlashlight.AssignOwner(photonView.Owner, playerBody);
                uvFlashlight.SetCurrentBattery(amount);
                droppedItem.GetComponent<PhotonView>().RPC("SyncBattery", RpcTarget.AllBuffered, amount);
            }

            droppedItem.GetComponent<PhotonView>().RPC("EnablePhysicsRPC", RpcTarget.AllBuffered);
        }
        else
        {
            // Drop standard pour objets sans batterie
            droppedItem = PhotonNetwork.Instantiate(itemPrefab.name, dropPosition, dropRotation);

            switch (selectedItem)
            {
				case "Match":
    				droppedItem.GetComponent<InteractionScripts.MatchBox>().matchesToAdd = Mathf.FloorToInt(amount);
					droppedItem.transform.localScale = Vector3.one * 0.21f;
    				break;
                case string key when key.Contains("Key"):
                    droppedItem.GetComponent<InteractionScripts.Key>()?.DropKey();
                    break;
				case "Candle":
					droppedItem.GetComponent<InteractionScripts.Candle>().candlesToAdd  = Mathf.FloorToInt(amount);
                	break;
                case "Lighter":
                    droppedItem.GetComponent<InteractionScripts.Lighter>()?.DropLighter();
                    break;
                case "Battery":
					droppedItem.GetComponent<InteractionScripts.Battery>().batteryCharge = amount;
                    droppedItem.GetComponent<InteractionScripts.Battery>()?.DropBattery();
                    break;
            }

			inventory.RemoveItem(selectedItem, amount);

            // Ajout du Rigidbody
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb == null && selectedItem != "Battery")
            {
                rb = droppedItem.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Sécurité sur le collider
        Collider col = droppedItem.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(EnableColliderWithDelay(col, 0.3f));
        }

        droppedItem.transform.position += Vector3.up * 0.05f;

        Debug.Log($"{selectedItem} a été jeté !");
    }

    private IEnumerator EnableColliderWithDelay(Collider col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null)
        {
            col.enabled = true;
        }
    }

    public void DropItemByName(string itemName, bool stack)
    {
        DropSelectedItem(stack, itemName);
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
    
    public bool IsItemEquipped(string itemName)
    {
        return equippedItems.ContainsKey(itemName) && equippedItems[itemName];
    }
    
	private IEnumerator UseCooldown()
	{
    	useCooldown = true;
    	yield return new WaitForSeconds(0.2f);
    	useCooldown = false;
	}

	[PunRPC]
	private void DisableItemColliderRPC(int viewID)
	{
    	PhotonView targetView = PhotonView.Find(viewID);
    	if (targetView != null)
    	{
        	Collider col = targetView.GetComponent<Collider>();
        	if (col != null)
        	{
            	col.enabled = false;
        	}
    	}
	}
    
    public void ForceUnequipItem(string itemName)
    {
        if (equippedItems.ContainsKey(itemName) && equippedItems[itemName])
        {
            equippedItems[itemName] = false;

            switch (itemName)
            {
                case "Flashlight":
                    if (flashlightScript != null)
                    {
                        flashlightScript.UnequipFlashlight(inventory);
                        flashlightScript = null;
                    }
                    break;
                case "UVFlashlight":
                    if (UVFlashlightScript != null)
                    {
                        UVFlashlightScript.UnequipFlashlight(inventory);
                        UVFlashlightScript = null;
                    }
                    break;
                case "Wrench":
                    if (wrenchScript != null)
                    {
                        wrenchScript.ShowWrench(false);
                        wrenchScript = null;
                    }
                    break;
                case "Crowbar":
                    if (crowbarScript != null)
                    {
                        crowbarScript.ShowCrowbar(false);
                        crowbarScript = null;
                    }
                    break;
                case "Magnetophone":
                    if (magnetophoneScript != null)
                    {
                        magnetophoneScript.ShowMagnetophone(false);
                        magnetophoneScript = null;
                    }
                    break;
                case "EMFDetector":
                    if (emfScript != null)
                    {
                        emfScript.ShowEMF(false);
                        emfScript = null;
                    }
                    break;
                case "CDDisk":
                    if (diskScript != null)
                    {
                        diskScript.ShowDisk(false);
                        diskScript = null;
                    }
                    break;
            }

            inventory.UpdateActionText();
        }
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
            return;
        
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

        float batteryLevel = inventory.GetItemCount("Flashlight");
		object[] instantiationData = new object[] { batteryLevel };

		GameObject flashlightInstance = PhotonNetwork.Instantiate(flashlightPrefab.name, Vector3.zero, flashlightRotation, 0, instantiationData);

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

	public InteractionScripts.Flashlight GetEquippedFlashlight()
	{
    	return flashlightScript;
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

		float batteryLevel = inventory.GetItemCount("UVFlashlight");
		object[] instantiationData = new object[] { batteryLevel };

		GameObject UVFlashlightInstance = PhotonNetwork.Instantiate(UVFlashlightPrefab.name, Vector3.zero, UVFlashlightRotation, 0, instantiationData);

        PhotonView uvView = UVFlashlightInstance.GetComponent<PhotonView>();
		uvView.RPC("SetColliderState", RpcTarget.AllBuffered, false);

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

	public InteractionScripts.UVFlashlight GetEquippedUVFlashlight()
	{
    	return UVFlashlightScript;
	}
    
    // Gestion wrench
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
        photonView.RPC("DisableItemColliderRPC", RpcTarget.AllBuffered, wrenchInstance.GetComponent<PhotonView>().ViewID);

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

        GameObject magnetophoneInstance = PhotonNetwork.Instantiate(magnetophonePrefab.name, Vector3.zero, magnetophoneRotation);
        photonView.RPC("DisableItemColliderRPC", RpcTarget.AllBuffered, magnetophoneInstance.GetComponent<PhotonView>().ViewID);

        yield return new WaitForSeconds(0.1f);

        magnetophoneScript = magnetophoneInstance.GetComponent<InteractionScripts.Magnetophone>();

        magnetophoneScript.AssignOwner(photonView.Owner, playerBody);
		magnetophoneScript.ShowMagnetophone(true);
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
            emfScript.ShowEMF(false);
            emfScript = null;
        }
    }

    private IEnumerator SpawnEMFDetector()
    {
        Quaternion emfRotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);

        GameObject emfInstance = PhotonNetwork.Instantiate(emfPrefab.name, Vector3.zero, emfRotation);
        photonView.RPC("DisableItemColliderRPC", RpcTarget.AllBuffered, emfInstance.GetComponent<PhotonView>().ViewID);

        yield return new WaitForSeconds(0.1f);

        emfScript = emfInstance.GetComponent<InteractionScripts.EMFDetector>();

        emfScript.AssignOwner(photonView.Owner, playerBody);
		emfScript.ShowEMF(true);
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
        photonView.RPC("DisableItemColliderRPC", RpcTarget.AllBuffered, diskInstance.GetComponent<PhotonView>().ViewID);

        yield return new WaitForSeconds(0.1f);

        diskScript = diskInstance.GetComponent<InteractionScripts.CDDisk>();

        diskScript.AssignOwner(photonView.Owner, playerBody);
        diskScript.ShowDisk(true);
    }
    
    public bool HasCDInHand()
    {
        return diskScript != null && diskScript.isTaken;
    }
    
    // Gestion crowbar
    [PunRPC]
    private void UseCrowbar()
    {
        if (!photonView.IsMine) 
            return;
        
        if (crowbarScript == null && inventory.HasItem("Crowbar"))
        {
            StartCoroutine(SpawnCrowbar());
        }
        else if (crowbarScript != null)
        {
            crowbarScript.ShowCrowbar(false);
            crowbarScript = null;
        }
    }

    private IEnumerator SpawnCrowbar()
    {
        Quaternion crowbarRotation = Quaternion.Euler(-90f, playerBody.eulerAngles.y, 0f);

        GameObject crowbarInstance = PhotonNetwork.Instantiate(crowbarPrefab.name, Vector3.zero, crowbarRotation);
        photonView.RPC("DisableItemColliderRPC", RpcTarget.AllBuffered, crowbarInstance.GetComponent<PhotonView>().ViewID);

        yield return new WaitForSeconds(0.1f);

        crowbarScript = crowbarInstance.GetComponent<InteractionScripts.Crowbar>();

        crowbarScript.AssignOwner(photonView.Owner, playerBody);
        crowbarScript.ShowCrowbar(true);
    }
}