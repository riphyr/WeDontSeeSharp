using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class CaptureLock : MonoBehaviourPun
    {
        public GameObject targetToFree;
        public GameObject playerToUnchain;

        [Header("Sons")]
        public AudioClip unlockSuccess;
        public AudioClip unlockFail;

        private AudioSource audioSource;
        private PhotonView view;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
        }

        public void TryUnlock(PlayerInventory inventory)
        {
            if (inventory.HasItem("Capture key"))
            {
                inventory.RemoveItem("Capture key", 1);
                view.RPC("RPC_Unlock", RpcTarget.AllBuffered);
            }
            else
            {
                audioSource.PlayOneShot(unlockFail);
            }
        }

        [PunRPC]
        private void RPC_Unlock()
        {
            StartCoroutine(UnlockAfterSound());
        }

        private IEnumerator UnlockAfterSound()
        {
            audioSource.PlayOneShot(unlockSuccess);
            yield return new WaitForSeconds(unlockSuccess.length);

            if (targetToFree != null)
                targetToFree.SetActive(false);

            if (playerToUnchain != null && GhostCatchManager.Instance != null)
                GhostCatchManager.Instance.UnchainPlayer(playerToUnchain);

            Destroy(gameObject);
        }
    }
}