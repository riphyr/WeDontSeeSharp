using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class MatchBox : MonoBehaviourPun
    {
        public int matchesToAdd = 5;
        private AudioSource audioSource;
        public AudioClip pickupSound;

        private PhotonView view;

        void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
        }

        public void PickupMatchBox(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
            
            inventory.AddItem("Match", matchesToAdd);
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
    }
}