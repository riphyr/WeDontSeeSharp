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
    public GameObject playerPrefab; // Prefab du player a spawn
    public string requiredSceneName = "GameScene"; // Scene du spawn
    private bool playerSpawned = false; // le joueur a-t-il spawn ?
    public GameObject pointRedPrefab; // Point rouge repère pour la MiniMap de depart
    public GameObject pointRedObject; // Point rouge du joueur 
    private Transform playerTarget; // Regarder ca sert a quoi ?
    
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
    
    void Update()
    {
        if (playerTarget != null && pointRedPrefab != null)
        {
            Vector3 newPos = playerTarget.position + new Vector3(0, 15f, 0);
            pointRedPrefab.transform.position = newPos;
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
            MaxPlayers = 4,  // Roome limitée a 4 joueurs ( en raison de la table de poker )
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
        Vector3 Position = new Vector3(-31, 10, -56);
        
        // Vérifie si un spawn ID a été défini
        string spawnID = PlayerPrefs.GetString("SpawnPointID", "");
        Dictionary<string, Vector3> spawnPositions = new Dictionary<string, Vector3>()
        {
            { "porte1", new Vector3(-2f, 2.5f, -6f) },  // Maison d'enfance 
            { "porte2", new Vector3(-38, 4, 26) }, // Labyrinthe
            { "porte3", new Vector3(100, 100, 100) }, // Chambre d'enfant 
            { "porte4", new Vector3(21, 5, 6) }, // Studio
            { "hub", new Vector3(70, -39, -54) }  // Hub 
        };

        if (!string.IsNullOrEmpty(spawnID) && spawnPositions.ContainsKey(spawnID))
        {
            Position = spawnPositions[spawnID];
        }
        
        // Instanciation du prefab du joueur et tag
        GameObject myPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Position, Quaternion.identity);
        PhotonNetwork.LocalPlayer.TagObject = myPlayer;
        
        PlayerPrefs.DeleteKey("SpawnPointID");
        PlayerPrefs.Save();
        

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
        ConsoleManager consoleManager = player.GetComponent<ConsoleManager>();
        MiniMapController miniMapController = player.GetComponent<MiniMapController>();
        
        //Activation de la MiniMap du debut de jeu 
        if(string.IsNullOrEmpty(spawnID))
            miniMapController.ShowMiniMap();

        if (playerScript == null) Debug.LogError("🚨 PlayerScript manquant sur Player !");
        if (cameraLookingAt == null) Debug.LogError("🚨 CameraLookingAt manquant sur Player !");
        if (playerInventory == null) Debug.LogError("🚨 PlayerInventory manquant sur Player !");
        if (playerInventoryUI == null) Debug.LogError("🚨 PlayerInventoryUI manquant sur Player !");
        if (playerJournalUI == null) Debug.LogError("🚨 PlayerJournalUI manquant sur Player !");
        if (playerUsing == null) Debug.LogError("🚨 PlayerUsing manquant sur Player !");
        if (pauseMenuManager == null) Debug.LogError("🚨 PauseMenuManager manquant sur Player !");
        if (consoleManager == null) Debug.LogError("🚨 ConsoleManager manquant sur Player !");

        playerScript.enabled = true;
        cameraLookingAt.enabled = true;
        playerInventory.enabled = true;
        playerInventoryUI.enabled = true;
        playerJournalUI.enabled = true;
        playerUsing.enabled = true;
        pauseMenuManager.enabled = true;
        consoleManager.enabled = true;

        // Activation de la caméra individuelle
        GameObject mainCamera = player.transform.Find("Main Camera").gameObject;
        mainCamera.SetActive(true);

        // Activation de l'affichage d'ouverture de porte
        GameObject interactionMenu = myPlayer.transform.Find("GUI").gameObject;
        interactionMenu.SetActive(true);
        
        // Instancier le point rouge
        GameObject pointRed = Instantiate(pointRedPrefab);  
        pointRed.transform.SetParent(player.transform);
        pointRed.transform.localPosition = new Vector3(0, 15, 0);
        
        // Montre-le uniquement si c’est toi
        PhotonView view = player.GetComponent<PhotonView>();
        if (view.IsMine)
        {
            pointRed.SetActive(true);
        }
        else
        {
            pointRed.SetActive(false);
        
        }
        pointRedObject = pointRed;
        playerTarget = player.transform;
    }
}
