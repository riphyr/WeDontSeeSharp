using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Key : MonoBehaviourPun
    {
        [Header("Paramètres de la clé")]
        public string keyName = "DefaultKey";

        [Header("Audio")] 
        public AudioSource audioSource;
        public AudioClip keyPickupSound;
        
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
        }
        
        public void PickupKey(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
            
            inventory.AddItem(keyName, 1);
            audioSource.PlayOneShot(keyPickupSound, 1.0f);
            view.RPC("DisableKeyForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DisableKeyForAll()
        {
            if (transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }

            if (TryGetComponent<BoxCollider>(out BoxCollider collider))
            {
                collider.enabled = false;
            }

        }
    }
}