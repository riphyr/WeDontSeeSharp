using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Battery : MonoBehaviourPun
    {
        public float batteryCharge = 100f; // Charge d'une pile
        public AudioClip pickupSound;
        private AudioSource audioSource;
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();

        }

        public void PickupBattery(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("Battery", (int)batteryCharge);
            audioSource.PlayOneShot(pickupSound);
            photonView.RPC("PlayPickupSound", RpcTarget.Others);
            StartCoroutine(DestroyAfterSound());
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            audioSource.PlayOneShot(pickupSound);
        }

        private IEnumerator DestroyAfterSound()
        {
            yield return new WaitForSeconds(pickupSound.length);
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DestroyForAll()
        {
            Destroy(gameObject);
        }
    }
}