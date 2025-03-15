using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PauseMenu;

public class SpawnPlayers2 : MonoBehaviour
{
    public GameObject playerPrefab;
    
    private void Start()
    {
        // Définition du spawnpoint
        Vector3 Position = new Vector3(-50, 3, 26);
        // Instanciation du prefab du joueur
        GameObject myPlayer = (GameObject) PhotonNetwork.Instantiate(playerPrefab.name, Position, Quaternion.identity);
        
        // Activation de la camera et des mouvements locaux
        GameObject player = myPlayer.transform.Find("Player").gameObject;
        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        playerScript.enabled = true;
        
        GameObject pauseMenu = myPlayer.transform.Find("Pause_Menu").gameObject;
        PauseMenuManager pauseMenuScript = pauseMenu.GetComponent<PauseMenuManager>();
        pauseMenuScript.enabled = true;
        
        GameObject mainCamera = player.transform.Find("Main Camera").gameObject;
        mainCamera.SetActive(true);
    }
    
}