using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    public class DissolvableNote : MonoBehaviourPun
    {
        [Header("Matériaux")]
        public Renderer noteRenderer;
        public Material cleanMaterial;
        public Material revealedMaterial;

        private bool isRevealed = false;

        private void Start()
        {
            if (noteRenderer != null && cleanMaterial != null)
            {
                noteRenderer.material = cleanMaterial;
            }
        }
        
        public bool IsRevealed => isRevealed;

        public void TryReveal(PlayerInventory inventory)
        {
            if (isRevealed) return;

            if (inventory.HasItem("Solvent"))
            {
                inventory.RemoveItem("Solvent", 1);
                photonView.RPC(nameof(RPC_RevealMaterial), RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        private void RPC_RevealMaterial()
        {
            if (isRevealed) return;

            isRevealed = true;

            if (noteRenderer != null && revealedMaterial != null)
            {
                noteRenderer.material = revealedMaterial;
            }
        }
    }
}