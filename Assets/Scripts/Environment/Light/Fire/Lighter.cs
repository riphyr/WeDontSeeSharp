using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Lighter : MonoBehaviourPun
    {
        public AudioClip pickupSound;
        private AudioSource audioSource;
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
        }

        public void PickupLighter(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("Lighter", 1);
            photonView.RPC("PlayPickupSound", RpcTarget.All);
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            StartCoroutine(PlaySoundAndDisable());
        }

        private IEnumerator PlaySoundAndDisable()
        {
            audioSource.PlayOneShot(pickupSound);
            yield return new WaitForSeconds(pickupSound.length);
            photonView.RPC("DisableForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DisableForAll()
        {
            gameObject.SetActive(false);
        }
    }
}