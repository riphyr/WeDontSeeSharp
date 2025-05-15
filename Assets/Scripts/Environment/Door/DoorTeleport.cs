using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class DoorTeleport : MonoBehaviour
{
    public string doorID;
    public string requiredPreviousDoorID;
    public string targetSceneName;
    public GameObject smokePrefab;
    //public LoadingManager loadingManager;
    public TMP_Text waitingMessageText; // Référence au Text UI où le message sera affiché

    private bool hasTeleported = false;
    private HashSet<string> playersPassed = new HashSet<string>();
    private int totalPlayers;
    private PhotonView photonView;
    private GameObject smoke; 

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    }
    
    public bool IsPreviousDoorDone()
    {
        if (string.IsNullOrEmpty(requiredPreviousDoorID))
            return true;

        return GameProgressManager.Instance.levelCompleted.ContainsKey(requiredPreviousDoorID);
    }

    public void OnDoorOpened()
    {
        // Vérifie si la porte précédente a été franchie par tous les joueurs
        if (!string.IsNullOrEmpty(requiredPreviousDoorID))
        {
            if (PlayerPrefs.GetInt(requiredPreviousDoorID, 0) != 1)
            {
                Debug.LogWarning($"La porte {doorID} est verrouillée car {requiredPreviousDoorID} n'est pas encore franchie !");
                return; // Empêche d'ouvrir la porte si la précédente n'est pas franchie
            }
        }
        else
        {
            // Si aucune porte précédente n'est spécifiée (c'est la première porte), on peut l'ouvrir
            Debug.Log("Première porte : aucune porte précédente à franchir.");
        }
        // Joue un effet de fumée à l'ouverture
        if (smokePrefab != null)
        {
            Vector3 offset = new Vector3(0, 1f, 0); // 1 mètre au-dessus
            smoke = Instantiate(smokePrefab, transform.position - offset, Quaternion.identity);
        }
    }

    public void OnDoorClosed()
    {
        if (smoke != null)
        {
            Destroy(smoke);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView playerPV = other.GetComponent<PhotonView>();
        if (playerPV == null || !playerPV.IsMine) return;

        string userId = PhotonNetwork.LocalPlayer.UserId;

        // Vérification spécifique pour la scène solo (porte2)
        if (doorID == "porte2")
        {
            if (!GameProgressManager.Instance.CanEnterSoloLevel(userId))
            {
                Debug.Log("Un autre joueur est déjà dans le niveau solo !");
                if (waitingMessageText != null)
                {
                    waitingMessageText.text = "Un autre joueur explore déjà ce niveau.";
                }
                return; // On ne téléporte pas
            }
            else
            {
                GameProgressManager.Instance.EnterSoloLevel(userId);
            }
        }

        // On envoie l'info à tout le monde que ce joueur est dans la zone
        photonView.RPC("PlayerPassedRPC", RpcTarget.AllBuffered, userId);
    }


    [PunRPC]
    public void PlayerPassedRPC(string userId)
    {
        if (doorID == "porte2")
        {
            if (PhotonNetwork.LocalPlayer.UserId == userId)
            {
                // Vérifie côté serveur si ce joueur a déjà terminé cette porte
                if (GameProgressManager.Instance.HasPlayerCompletedSoloLevel(userId, doorID))
                {
                    Debug.Log("Ce joueur a déjà fait " + doorID);
                    return;
                }

                // Enregistrement
                GameProgressManager.Instance.RegisterSoloLevelCompletion(userId, doorID);

                // Localement
                PlayerPrefs.SetInt("Passed_" + doorID, 1);
                PlayerPrefs.SetString("SpawnPointID", doorID);
                PlayerPrefs.Save();

                TeleportSinglePlayer(userId);
            }
            return;
        }


        // Cas multijoueur normal
        if (!playersPassed.Contains(userId))
        {
            playersPassed.Add(userId);
            Debug.Log($"Player {userId} a franchi la porte {doorID}");

            if (playersPassed.Count >= totalPlayers)
            {
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    string playerId = player.UserId;
                    GameProgressManager.Instance.RegisterReturnToHub(playerId, doorID);
                }
                PlayerPrefs.SetString("SpawnPointID", doorID);
                PlayerPrefs.Save();
                photonView.RPC("TeleportAllPlayers", RpcTarget.All);
            }
            else
            {
                photonView.RPC("DisplayWaitingMessage", RpcTarget.All);
            }
        }
    }

    
    
    private void TeleportSinglePlayer(string userId)
    {
        if (PhotonNetwork.LocalPlayer.UserId == userId)
        {
            hasTeleported = true;
            LoadingManager.Instance.StartLoading(targetSceneName); // Ex: "porte2"
        }
    }
    [PunRPC]
    private void DisplayWaitingMessage()
    {
        // Afficher le message à tous les joueurs dans la salle noire
        if (waitingMessageText != null)
        {
            waitingMessageText.text = "Wait for all players";
        }
    }

    [PunRPC]
    private void TeleportAllPlayers()
    {
        if (hasTeleported) return;

        hasTeleported = true;

        //string userId = PhotonNetwork.LocalPlayer.UserId;
        LoadingManager.Instance.StartLoading(targetSceneName);
    }
    
}

