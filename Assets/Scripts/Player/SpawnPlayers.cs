using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PauseMenu;
using Studio;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public string requiredSceneName = "GameScene";
    private bool playerSpawned = false;
    public Consigne consigne;  // Référence à la classe Consigne

    
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

        SpawnPlayer();
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

        PhotonNetwork.JoinOrCreateRoom("DefaultRoom", roomOptions, TypedLobby.Default);
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
        Vector3 Position = new Vector3(22, 5, 8);

        // Instanciation du prefab du joueur avec position et rotation
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
        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        CameraLookingAt cameraLookingAt = player.GetComponent<CameraLookingAt>();
        PlaceCandle placeCandle = player.GetComponent<PlaceCandle>();
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        PlayerUsing playerUsing = player.GetComponent<PlayerUsing>();

        playerScript.enabled = true;
        cameraLookingAt.enabled = true;
        playerInventory.enabled = true;
        placeCandle.enabled = true;
        playerUsing.enabled = true;

        // Activation du menu de pause
        GameObject pauseMenu = myPlayer.transform.Find("Pause_Menu").gameObject;
        PauseMenuManager pauseMenuScript = pauseMenu.GetComponent<PauseMenuManager>();
        pauseMenuScript.enabled = true;

        // Activation de la caméra individuelle
        GameObject mainCamera = player.transform.Find("Main Camera").gameObject;
        mainCamera.SetActive(true);

        // Activation de l'affichage d'ouverture de porte
        GameObject interactionMenu = myPlayer.transform.Find("GUI").gameObject;
        interactionMenu.SetActive(true);

        if (requiredSceneName == "Scene_02")
        {
            consigne.Text();
        }
    }
}
