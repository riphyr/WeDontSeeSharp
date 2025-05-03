using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class Diary : MonoBehaviourPun
    {
        [Header("Paramètres principaux")]
        public string itemName = "Diary";
        public AudioClip pickupSound;
        public LoreProgressManager loreProgress;

        private PhotonView view;
        private AudioSource audioSource;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            view.OwnershipTransfer = OwnershipOption.Takeover;
            audioSource = GetComponent<AudioSource>();
        }

        public bool CanShowInteractionText()
        {
            return !loreProgress.IsDiaryTaken() && loreProgress.IsDiaryUnlocked();
        }

        public void TryPickup(PlayerInventory inventory)
        {
            if (!view.IsMine)
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            
            if (loreProgress.IsDiaryTaken() || !loreProgress.IsDiaryUnlocked())
                return;

            inventory.AddItem(itemName);
            loreProgress.SetDiaryTaken();

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