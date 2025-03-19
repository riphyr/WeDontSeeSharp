using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Wrench : MonoBehaviourPun
    {
        private Transform ownerTransform;
        private Camera playerCamera;
        
        private AudioSource audioSource;
        public AudioClip pickupSound;
        public AudioClip useSound;

        private PhotonView view;
        private bool isTaken = false;

        void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
        }

        public void PickupWrench(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
            
            inventory.AddItem("Wrench");
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
        
        public void AssignOwner(Photon.Realtime.Player player, Transform ownerTransform)
        {
            this.ownerTransform = ownerTransform;
            photonView.TransferOwnership(player);
        }
        
        void Update()
        {
            if (ownerTransform != null)
            {
                transform.position = ownerTransform.position + ownerTransform.forward * 0.3f + ownerTransform.right * 0.1f + Vector3.up * 0.6f;
                transform.rotation = Quaternion.Euler(0f, ownerTransform.eulerAngles.y, 0f);
            }
        }
        
        public void ShowWrench (bool show)
        {
            isTaken = show;
            
            if (!show)
            {
                photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
            }
        }
        
        [PunRPC]
        private void PlayUseSound()
        {
            StartCoroutine(PlaySound());
        }
        
        private IEnumerator PlaySound()
        {
            audioSource.PlayOneShot(useSound);
            yield return new WaitForSeconds(useSound.length);
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                transform.position = (Vector3)stream.ReceiveNext();
                transform.rotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}