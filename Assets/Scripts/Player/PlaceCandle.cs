using UnityEngine;
using System.Collections;
using Photon.Pun;

public class PlaceCandle : MonoBehaviourPun
{
    public GameObject candlePrefab;
    public float maxPlaceDistance = 3f;
    private GameObject previewCandle;
    private bool placingCandle = false;
    private PlayerInventory inventory;
    private Camera playerCamera;
    private CameraLookingAt cameraLookingAt;
    private BoxCollider box;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        playerCamera = GetComponentInChildren<Camera>();
        cameraLookingAt = GetComponentInChildren<CameraLookingAt>();
    }

    void Update()
    {
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
    }

    private void TryToPlaceCandle()
    {
        if (inventory.HasItem("Candle"))
        {
            cameraLookingAt.enabled = false;
            
            placingCandle = true;
            previewCandle = PhotonNetwork.Instantiate(candlePrefab.name, transform.position, Quaternion.identity);
            
            box = previewCandle.GetComponent<BoxCollider>();
            box.enabled = false;
        }
    }

    private void UpdateCandlePosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;
        
        Debug.DrawRay(ray.origin, ray.direction * maxPlaceDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, maxPlaceDistance))
        {
            previewCandle.transform.position = hit.point + new Vector3(-0.05f, 0.125f, -0.05f);
        }
    }

    private void ConfirmCandlePlacement()
    {
        placingCandle = false;
        inventory.RemoveItem("Candle", 1);
        cameraLookingAt.enabled = true;
        box.enabled = true;
    }
}