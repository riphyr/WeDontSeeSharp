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
                Debug.Log("🔌 Joueur non connecté à Photon, tentative de connexion...");
                PhotonNetwork.ConnectUsingSettings();
            }
        }

<<<<<<< HEAD
        else if (PhotonNetwork.InRoom && !playerSpawned)
        {
            SpawnPlayer();
            playerSpawned = true;
        }
=======
        SpawnPlayer();
>>>>>>> emmarucay-patch-1
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Connecté au serveur Photon !");
        PhotonNetwork.JoinLobby();
    }
    
    public override void OnJoinedLobby()
    {
        Debug.Log("📢 Joueur a rejoint le lobby. Cherche une Room...");
        JoinOrCreateRoom();
    }
    
    private void JoinOrCreateRoom()
    {
        Debug.Log("🛠️ Tentative de rejoindre/créer une Room...");

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 10,  // Définir un max de joueurs
            IsVisible = true, // La Room est visible par les autres joueurs
            IsOpen = true     // La Room peut être rejointe
        };

<<<<<<< HEAD
        PhotonNetwork.JoinOrCreateRoom("a", roomOptions, TypedLobby.Default);
=======
        PhotonNetwork.JoinOrCreateRoom("DefaultRoom", roomOptions, TypedLobby.Default);
>>>>>>> emmarucay-patch-1
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("🏠 Joueur a rejoint une Room. Spawn en cours...");
        if (!playerSpawned)
        {
            SpawnPlayer();
            playerSpawned = true;
        }
    }
    
    private void SpawnPlayer()
    {
        // Définition du spawnpoint
<<<<<<< HEAD
        Vector3 Position = new Vector3(4, 4, 1);
=======
        Vector3 Position = new Vector3(-35, 3, 44);
>>>>>>> emmarucay-patch-1

        // Instanciation du prefab du joueur et tag
        GameObject myPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Position, Quaternion.identity);
        
        PhotonNetwork.LocalPlayer.TagObject = myPlayer;

        //Check pour la version de test
        if (!myPlayer.activeSelf)
        {
            Debug.LogWarning("⚠️ Le joueur était désactivé, activation en cours...");
            myPlayer.SetActive(true);
        }
        
        // Activation des scripts de mouvement et d'interactions
        GameObject player = myPlayer.transform.Find("Player").gameObject;
<<<<<<< HEAD
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
=======
        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        CameraLookingAt cameraLookingAt = player.GetComponent<CameraLookingAt>();
        PlaceCandle placeCandle = player.GetComponent<PlaceCandle>();
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        PlayerUsing playerUsing = player.GetComponent<PlayerUsing>();
>>>>>>> emmarucay-patch-1

        playerScript.enabled = true;
        cameraLookingAt.enabled = true;
        playerInventory.enabled = true;
<<<<<<< HEAD
        playerInventoryUI.enabled = true;
        playerJournalUI.enabled = true;
        playerUsing.enabled = true;
        pauseMenuManager.enabled = true;
=======
        placeCandle.enabled = true;
        playerUsing.enabled = true;

        // Activation du menu de pause
        GameObject pauseMenu = myPlayer.transform.Find("Pause_Menu").gameObject;
        PauseMenuManager pauseMenuScript = pauseMenu.GetComponent<PauseMenuManager>();
        pauseMenuScript.enabled = true;
>>>>>>> emmarucay-patch-1

        // Activation de la caméra individuelle
        GameObject mainCamera = player.transform.Find("Main Camera").gameObject;
        mainCamera.SetActive(true);

        // Activation de l'affichage d'ouverture de porte
        GameObject interactionMenu = myPlayer.transform.Find("GUI").gameObject;
        interactionMenu.SetActive(true);
    }
}
