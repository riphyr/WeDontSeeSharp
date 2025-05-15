using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class Trunk : MonoBehaviourPun
    {
        public enum TrunkType
        {
            DropsDownThenOpens,
            OpensDirectly
        }

        [Header("Configuration")]
        [SerializeField] private TrunkType trunkType;
        [SerializeField] private AudioClip moveSound;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private Animator animator;
        [SerializeField] private LoreProgressManager loreProgress;

        private PhotonView view;
        private AudioSource audioSource;
        private bool hasMoved = false;
        private bool hasOpened = false;

        public bool HasMoved => hasMoved;
        public bool HasOpened => hasOpened;
        public TrunkType Type => trunkType;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            view.OwnershipTransfer = OwnershipOption.Takeover;
        }

        public bool CanShowInteractionText()
        {
            if (trunkType == TrunkType.DropsDownThenOpens)
                return loreProgress.IsTrunk1Unlocked();
            else if (trunkType == TrunkType.OpensDirectly)
                return loreProgress.IsTrunk2Unlocked();
            return false;
        }

        public void TryInteract()
        {
            if (!view.IsMine)
                view.TransferOwnership(PhotonNetwork.LocalPlayer);

            if (trunkType == TrunkType.DropsDownThenOpens)
            {
                if (!hasMoved)
                    view.RPC("RPC_MoveTrunk", RpcTarget.AllBuffered);
                else if (!hasOpened)
                    view.RPC("RPC_OpenTrunk", RpcTarget.AllBuffered);
            }
            else if (trunkType == TrunkType.OpensDirectly && !hasOpened)
            {
                view.RPC("RPC_OpenTrunk", RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        private void RPC_MoveTrunk()
        {
            if (hasMoved) return;

            hasMoved = true;
            animator.SetTrigger("Move");
            audioSource.PlayOneShot(moveSound, 0.5f);
            loreProgress.SetTrunk1Moved();
        }

        [PunRPC]
        private void RPC_OpenTrunk()
        {
            if (hasOpened) return;

            hasOpened = true;
            animator.SetTrigger("Open");
            audioSource.PlayOneShot(openSound);

            if (trunkType == TrunkType.DropsDownThenOpens)
                loreProgress.SetTrunk1Opened();
            else if (trunkType == TrunkType.OpensDirectly)
                loreProgress.SetTrunk2Opened();
        }
    }
}
