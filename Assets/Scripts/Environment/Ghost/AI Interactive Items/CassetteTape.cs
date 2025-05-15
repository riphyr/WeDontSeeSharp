using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class CassetteTape : MonoBehaviourPun
    {
        [Header("Paramètres principaux")]
        public string itemName = "Cassette";
        public AudioClip pickupSound;
        public LoreProgressManager loreProgress;

        private PhotonView view;
        private AudioSource audioSource;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            view.OwnershipTransfer = OwnershipOption.Takeover;
        }

        public bool CanShowInteractionText()
        {
            return !loreProgress.IsCassetteTaken() && loreProgress.IsTrunk2Opened();
        }

        public void TryPickup(PlayerInventory inventory)
        {
            if (!view.IsMine)
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            
            if (loreProgress.IsCassetteTaken() || !loreProgress.IsTrunk2Opened())
                return;

            inventory.AddItem(itemName);
            loreProgress.SetCassetteTaken();

            view.RPC("PlayPickupSound", RpcTarget.All);
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            StartCoroutine(PlaySoundAndDestroy());
        }

        private IEnumerator PlaySoundAndDestroy()
        {
            audioSource.PlayOneShot(pickupSound);
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