using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PauseMenu;

public class PrivateSceneAccess : MonoBehaviourPun
{
    [Header("Prefab du joueur")]
    public GameObject playerPrefab;

    [Header("Position spéciale pour la scène privée")]
    public Vector3 teleportPosition = new Vector3(0, 2, 0);

    [Header("Nom de la propriété utilisée pour bloquer l'accès")]
    public string sceneLockKey = "SceneOccupied";

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            TryEnterPrivateScene();
        }
    }

    void TryEnterPrivateScene()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(sceneLockKey, out object value))
        {
            if ((bool)value)
            {
                Debug.Log("🚫 La scène est déjà occupée !");
                return;
            }
        }

        // Marquer la scène comme occupée
        Hashtable props = new Hashtable();
        props[sceneLockKey] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        // Détruire l'ancien joueur s'il existe
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }

        // Faire spawn le nouveau joueur
        SpawnPrivatePlayer();
    }

    void SpawnPrivatePlayer()
    {
        GameObject myPlayer = PhotonNetwork.Instantiate(playerPrefab.name, teleportPosition, Quaternion.identity);

        // Activer les composants comme dans SpawnPlayers2
        GameObject player = myPlayer.transform.Find("Player").gameObject;
        player.GetComponent<PlayerScript>().enabled = true;

        GameObject pauseMenu = myPlayer.transform.Find("Pause_Menu").gameObject;
        pauseMenu.GetComponent<PauseMenuManager>().enabled = true;

        GameObject mainCamera = player.transform.Find("Main Camera").gameObject;
        mainCamera.SetActive(true);

        Debug.Log("✅ Joueur spawné dans la scène privée !");
    }

    public void ExitPrivateScene()
    {
        if (!photonView.IsMine) return;

        Debug.Log("↩️ Sortie de la scène privée");
        Hashtable props = new Hashtable();
        props[sceneLockKey] = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}
