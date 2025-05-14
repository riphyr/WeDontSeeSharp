using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class NearDoll : MonoBehaviour
{
    public bool isTriggered = false;
    private void OnTriggerEnter(Collider other)
    {
        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView.IsMine)
        {
            Debug.Log("UwU");
            isTriggered = true;
        }
    }
}