using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
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
        SceneManager.LoadScene("Lobby");
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
   
}
