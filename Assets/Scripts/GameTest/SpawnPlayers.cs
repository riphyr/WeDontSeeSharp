using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    
    private void Start()
    {
        // Définition du spawnpoint
        Vector3 Position = new Vector3(0, 1, 0);
        // Instanciation du prefab du joueur
        GameObject myPlayer = (GameObject) PhotonNetwork.Instantiate(playerPrefab.name, Position, Quaternion.identity);
        
        // Activation de la camera et des mouvements locaux
        ((MonoBehaviour)myPlayer.GetComponent("PlayerScript")).enabled = true;
        myPlayer.transform.Find("Main Camera").gameObject.SetActive(true);
        
    }
    
}
