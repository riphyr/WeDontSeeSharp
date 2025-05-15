using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class Radio : MonoBehaviourPun
    {
        [Header("Paramètres principaux")]
        public AudioClip radioSound;
        public AudioClip cassetteInsertionSound;
        public LoreProgressManager loreProgress;

        private AudioSource audioSource;
        private PhotonView view;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            view.OwnershipTransfer = OwnershipOption.Takeover;
            audioSource = GetComponent<AudioSource>();
        }

        public bool CanShowInteractionText(PlayerInventory inventory)
        {
            return loreProgress.IsCassetteTaken() && inventory.HasItem("Cassette");
        }

        public void TryPlayRadio(PlayerInventory inventory)
        {
            if (!view.IsMine)
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            
            if (!loreProgress.IsCassetteTaken() || !inventory.HasItem("Cassette"))
                return;

            inventory.RemoveItem("Cassette");
            loreProgress.SetRadioPlayed();
            view.RPC("RPC_PlayRadio", RpcTarget.All);
        }
        
        [PunRPC]
        private void RPC_PlayRadio()
        {
            StartCoroutine(PlaySound());
        }

        private IEnumerator PlaySound()
        {
            audioSource.PlayOneShot(cassetteInsertionSound);
            yield return new WaitForSeconds(cassetteInsertionSound.length);
            audioSource.PlayOneShot(radioSound);;
        }
    }
}