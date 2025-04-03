using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Item : MonoBehaviourPun
    {
        [Header("ITEM SETTINGS")]
        public string itemName = "";

        public bool toLower = true;

        [Header("AUDIO")] 
        private AudioSource audioSource;
        public AudioClip pickupSound;
        
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
            view.OwnershipTransfer = OwnershipOption.Takeover;
        }
        
        public void Pickup(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
            
            inventory.AddItem(itemName);
            photonView.RPC("PlayPickupSound", RpcTarget.All);
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

        public string GetItemName()
        {
            return (toLower) ? itemName.ToLower() : itemName;
        }
    }
}