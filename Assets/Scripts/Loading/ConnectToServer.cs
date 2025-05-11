using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [Header("First scene name")]
    [SerializeField] private string firstSceneName;
    
    void Start()
    {
        // Demarrage si le joueur est offline et delay si le joueur est déjà online
        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(LoadLobbyWithDelay(2f));
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    // Emulation d'un delay fictif avant le chargement de la scène
    private IEnumerator LoadLobbyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameModeManager.IsMultiplayer)
            SceneManager.LoadScene("Lobby");
        else
        {
            PhotonNetwork.CreateRoom("");

            if (GameModeManager.IsLoading)
            {
                GameSaveData save = GameSaveManager.Load();
                string scene = save.currentScene;
            
                if (scene == "")
                    PhotonNetwork.LoadLevel(firstSceneName);
                else
                    PhotonNetwork.LoadLevel(scene);
            }
            else
            {
                var save = new GameSaveData
                {
                    isMultiplayer = false,
                    roomName = "",
                    currentScene = "",
                    playTime = 0f
                };

                GameSaveManager.Save(save);
            
                PhotonNetwork.LoadLevel(firstSceneName);
            }
        }
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if (GameModeManager.IsMultiplayer)
            SceneManager.LoadScene("Lobby");
        else
        {
            PhotonNetwork.CreateRoom("");

            if (GameModeManager.IsLoading)
            {
                GameSaveData save = GameSaveManager.Load();
                string scene = save.currentScene;
            
                if (scene == "")
                    PhotonNetwork.LoadLevel(firstSceneName);
                else
                    PhotonNetwork.LoadLevel(scene);
            }
            else
            {
                var save = new GameSaveData
                {
                    isMultiplayer = false,
                    roomName = "",
                    currentScene = "",
                    playTime = 0f
                };

                GameSaveManager.Save(save);
            
                PhotonNetwork.LoadLevel(firstSceneName);
            }
        }
    }
}
