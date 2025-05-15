using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class Clothes : MonoBehaviourPun
    {
        [Header("Paramètres")]
        public LoreProgressManager loreProgress;
        public Animator animator;
        public string searchTriggerName = "Search";
        public AudioClip searchSound;

        private AudioSource audioSource;
        private PhotonView view;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            view.OwnershipTransfer = OwnershipOption.Takeover;
            audioSource = GetComponent<AudioSource>();
        }

        public bool CanShowInteractionText()
        {
            return loreProgress.IsClothesUnlocked() && !loreProgress.IsClothesSearched();
        }

        public void TrySearchClothes()
        {
            if (!view.IsMine)
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            
            if (!loreProgress.IsClothesUnlocked() || loreProgress.IsClothesSearched())
                return;

            loreProgress.SetClothesSearched();

            view.RPC("RPC_PlayAnimationAndSound", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_PlayAnimationAndSound()
        {
            animator.SetTrigger(searchTriggerName);
            audioSource.PlayOneShot(searchSound);
        }
    }
}