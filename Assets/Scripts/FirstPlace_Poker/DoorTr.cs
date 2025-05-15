using UnityEngine;
using Photon.Pun;

public class DoorTr : MonoBehaviour
{
    //public GameObject miniMapUI;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name+ " collided with DoorTr");
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name+ " entered");
            PhotonView photonView = other.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                MiniMapController miniMap = other.GetComponent<MiniMapController>();
                Debug.Log(miniMap== null);
                if (miniMap != null)
                {
                    miniMap.HideMiniMap();
                }
            }
        }
    }
}
