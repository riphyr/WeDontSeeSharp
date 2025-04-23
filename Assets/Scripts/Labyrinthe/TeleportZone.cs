using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class TeleportZone : MonoBehaviourPun
{
    public string sceneToLoad = "PrivateScene";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView view = other.GetComponent<PhotonView>();
        if (view != null && view.IsMine)
        {
            Debug.Log("💫 Téléportation du joueur");
            DoorController.SetDoorLock(true); // verrouille la porte
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}