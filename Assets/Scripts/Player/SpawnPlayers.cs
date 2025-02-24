using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PauseMenu;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    
    private void Start()
    {
        // Définition du spawnpoint
        Vector3 Position = new Vector3(0, 1, 0);
        // Instanciation du prefab du joueur
        GameObject myPlayer = (GameObject) PhotonNetwork.Instantiate(playerPrefab.name, Position, Quaternion.identity);
        
        // Activation des scripts de mouvement et d'ouverture de porte
        GameObject player = myPlayer.transform.Find("Player").gameObject;
        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        CameraOpenDoor cameraOpenDoor = player.GetComponent<CameraOpenDoor>();
        playerScript.enabled = true;
        cameraOpenDoor.enabled = true;
        
        // Activation du menu de pause
        GameObject pauseMenu = myPlayer.transform.Find("Pause_Menu").gameObject;
        PauseMenuManager pauseMenuScript = pauseMenu.GetComponent<PauseMenuManager>();
        pauseMenuScript.enabled = true;
        
        // Activation de la camera individuelle
        GameObject mainCamera = player.transform.Find("Main Camera").gameObject;
        mainCamera.SetActive(true);
        
        // Activation de l'affichage d'ouverture de porte
        GameObject openDoorMenu = myPlayer.transform.Find("Gui_OpenDoorTouch").gameObject;
        openDoorMenu.SetActive(true);
    }
}
