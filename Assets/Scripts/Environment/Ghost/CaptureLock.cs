using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    public class CaptureLock : MonoBehaviourPun
    {
        public GameObject targetToFree;
        public GameObject playerToUnchain;

        public void TryUnlock(PlayerInventory inventory)
        {
            if (inventory.HasItem("Capture key"))
            {
                inventory.RemoveItem("Capture key", 1);
                photonView.RPC("RPC_Unlock", RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        private void RPC_Unlock()
        {
            if (targetToFree != null)
                targetToFree.SetActive(false);

            if (playerToUnchain != null && GhostCatchManager.Instance != null)
                GhostCatchManager.Instance.UnchainPlayer(playerToUnchain);

            Destroy(gameObject);
        }
    }
}