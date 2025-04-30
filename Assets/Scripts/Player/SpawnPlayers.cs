using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PauseMenu;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public string requiredSceneName = "GameScene";
    private bool playerSpawned = false;
    
    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            if (SceneManager.GetActiveScene().name == requiredSceneName)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        else if (PhotonNetwork.InRoom && !playerSpawned)
        {
            SpawnPlayer();
            playerSpawned = true;
        }
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    
    public override void OnJoinedLobby()
    {
        JoinOrCreateRoom();
    }
    
    private void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 10,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom("a", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if (!playerSpawned)
        {
            SpawnPlayer();
            playerSpawned = true;
        }
    }
    
    private void SpawnPlayer()
    {
        // Définition du spawnpoint
        Vector3 Position = new Vector3(0, 2.5f, -4);

        // Instanciation du prefab du joueur et tag
        GameObject myPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Position, Quaternion.identity);
        
        PhotonNetwork.LocalPlayer.TagObject = myPlayer;

        //Check pour la version de test
        if (!myPlayer.activeSelf)
        {
            myPlayer.SetActive(true);
        }
        
        // Activation des scripts de mouvement et d'interactions
        GameObject player = myPlayer.transform.Find("Player").gameObject;
        PlayerScript.LocalPlayerTransform = player.transform;
        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        CameraLookingAt cameraLookingAt = player.GetComponent<CameraLookingAt>();
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        PlayerUsing playerUsing = player.GetComponent<PlayerUsing>();
        PauseMenuManager pauseMenuManager = player.GetComponent<PauseMenuManager>();
        PlayerInventoryUI playerInventoryUI = player.GetComponent<PlayerInventoryUI>();
        PlayerJournalUI playerJournalUI = player.GetComponent<PlayerJournalUI>();

        if (playerScript == null) Debug.LogError("🚨 PlayerScript manquant sur Player !");
        if (cameraLookingAt == null) Debug.LogError("🚨 CameraLookingAt manquant sur Player !");
        if (playerInventory == null) Debug.LogError("🚨 PlayerInventory manquant sur Player !");
        if (playerInventoryUI == null) Debug.LogError("🚨 PlayerInventoryUI manquant sur Player !");
        if (playerJournalUI == null) Debug.LogError("🚨 PlayerJournalUI manquant sur Player !");
        if (playerUsing == null) Debug.LogError("🚨 PlayerUsing manquant sur Player !");
        if (pauseMenuManager == null) Debug.LogError("🚨 PauseMenuManager manquant sur Player !");

        playerScript.enabled = true;
        cameraLookingAt.enabled = true;
        playerInventory.enabled = true;
        playerInventoryUI.enabled = true;
        playerJournalUI.enabled = true;
        playerUsing.enabled = true;
        pauseMenuManager.enabled = true;

        // Activation de la caméra individuelle
        GameObject mainCamera = player.transform.Find("Main Camera").gameObject;
        mainCamera.SetActive(true);

        // Activation de l'affichage d'ouverture de porte
        GameObject interactionMenu = myPlayer.transform.Find("GUI").gameObject;
        interactionMenu.SetActive(true);
    }
}
