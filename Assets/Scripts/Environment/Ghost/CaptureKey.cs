using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    public class CaptureKey : MonoBehaviourPun
    {
        public void PickupKey(PlayerInventory inventory)
        {
            inventory.AddItem("Capture key");
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DestroyForAll()
        {
            Destroy(gameObject);
        }
    }
}