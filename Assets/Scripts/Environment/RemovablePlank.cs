using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    public class RemovablePlank : MonoBehaviourPun
    {
        private bool isUnlocked = false;
        public bool IsUnlocked => isUnlocked;

        private bool isRemoved = false;
        public bool IsRemoved => isRemoved;

        private PhotonView view;
        private Animator animator;
        private AudioSource audioSource;

        [Header("Audio")]
        public AudioClip removalSound;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();

            view.OwnershipTransfer = OwnershipOption.Takeover;
        }

        public void TryRemovePlank()
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            photonView.RPC(nameof(RemovePlank), RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RemovePlank()
        {
            if (isRemoved) return;

            isRemoved = true;

            animator.SetTrigger("Remove");

            if (removalSound != null)
            {
                audioSource.PlayOneShot(removalSound, 1f);
            }
        }

        public void UnlockPlank()
        {
            photonView.RPC("RPC_UnlockPlank", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RPC_UnlockPlank()
        {
            isUnlocked = true;
        }
    }
}