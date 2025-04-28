using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class LockKey : MonoBehaviourPun
    {
        [Header("Cadenas à clef")]
        public string requiredKeyName = "DefaultKey";
        public AudioSource asource;
        public AudioClip unlockSuccess, unlockFail;

        private PhotonView view;

        void Start()
        {
            asource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
        }

        public void AttemptUnlock()
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            PlayerInventory inventory = FindLocalPlayerInventory();

            if (inventory.HasItem(requiredKeyName))
            {
                inventory.RemoveItem(requiredKeyName, 1);
                view.RPC("PlayUnlockSoundAndDisable", RpcTarget.All);
            }
            else
            {
                asource.PlayOneShot(unlockFail, 1.0f);
            }
        }

        private PlayerInventory FindLocalPlayerInventory()
        {
            if (PhotonNetwork.LocalPlayer.TagObject is GameObject localPlayer)
            {
                Transform playerTransform = localPlayer.transform.Find("Player");

                if (playerTransform != null)
                {
                    return playerTransform.GetComponent<PlayerInventory>();
                }
            }

            return null;
        }

        [PunRPC]
        private void PlayUnlockSoundAndDisable()
        {
            StartCoroutine(UnlockAfterSound());
        }

        private IEnumerator UnlockAfterSound()
        {
            asource.PlayOneShot(unlockSuccess, 1.0f);
            yield return new WaitForSeconds(unlockSuccess.length);

            if (view.IsMine)
            {
                view.RPC("DisableLockForAll", RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        private void DisableLockForAll()
        {
            gameObject.SetActive(false);
        }
    }
}
