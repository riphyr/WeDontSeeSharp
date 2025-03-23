using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Wrench : MonoBehaviourPun, IPunObservable
    {
        private Transform ownerTransform;
        private Camera playerCamera;
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        
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
        
        public void AssignOwner(Photon.Realtime.Player player, Transform newOwnerTransform)
        {
            this.ownerTransform = newOwnerTransform;
            photonView.TransferOwnership(player);

            // Envoi la mise à jour du owner à tous les clients
            photonView.RPC("RPC_SetOwnerTransform", RpcTarget.OthersBuffered, player.ActorNumber);
        }

        [PunRPC]
        private void RPC_SetOwnerTransform(int playerID)
        {
            Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerID);
            if (player != null)
            {
                ownerTransform = player.TagObject as Transform;
            }
        }
        
        void Update()
        {
            if (photonView.IsMine)
            {
                if (ownerTransform != null)
                {
                    transform.position = ownerTransform.position + ownerTransform.forward * 0.3f + ownerTransform.right * 0.1f + Vector3.up * 0.6f;
                    transform.rotation = Quaternion.Euler(0f, ownerTransform.eulerAngles.y, 0f);
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10);
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
        
        // **Ajout d'une synchronisation correcte de la position et rotation**
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}