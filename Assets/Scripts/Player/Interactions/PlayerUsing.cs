using UnityEngine;
using Photon.Pun;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerUsing : MonoBehaviourPun
{
    [Header("Prefabs")]
    public GameObject matchPrefab;
    public GameObject candlePrefab;
    public GameObject flashlightPrefab;

    [Header("Player")]
    private PlayerInventory inventory;
    private CameraLookingAt cameraLookingAt;
    private Camera playerCamera;
    private Transform playerBody;
    
    [Header("Candle")]
    private GameObject previewCandle;
    private bool placingCandle = false;
    private BoxCollider candleCollider;

    [Header("Flashlight")] 
    private InteractionScripts.Flashlight flashlightScript;
    private CapsuleCollider flashlightCollider;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        playerBody = transform;
        playerCamera = GetComponentInChildren<Camera>();
        cameraLookingAt = GetComponent<CameraLookingAt>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            photonView.RPC("UseMatch", RpcTarget.All);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            TryToPlaceCandle();
        }

        if (placingCandle)
        {
            UpdateCandlePosition();

            if (Input.GetKeyDown(KeyCode.E))
            {
                ConfirmCandlePlacement();
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            photonView.RPC("UseFlashlight", RpcTarget.All);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RechargeFlashlight();
        }
    }

    // Gestion allumettes
    [PunRPC]
    private void UseMatch()
    {
        if (!photonView.IsMine) 
            return;

        if (!inventory.HasItem("Match"))
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

        InteractionScripts.Match matchScript = matchInstance.GetComponent<InteractionScripts.Match>();

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
        }
    }

    private void UpdateCandlePosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;
        
        Debug.DrawRay(ray.origin, ray.direction * 3, Color.yellow);
        
        if (Physics.Raycast(ray, out hit, 3f))
        {
            previewCandle.transform.position = hit.point + new Vector3(-0.05f, 0.125f, -0.05f);
        }
    }

    private void ConfirmCandlePlacement()
    {
        placingCandle = false;
        inventory.RemoveItem("Candle", 1);
        cameraLookingAt.enabled = true;
        candleCollider.enabled = true;
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
}