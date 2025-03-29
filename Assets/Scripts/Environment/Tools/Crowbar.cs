using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Crowbar : MonoBehaviourPun, IPunObservable
    {
        private Transform ownerTransform;
        private AudioSource audioSource;
        private Vector3 networkPosition;
        private Quaternion networkRotation;

        public AudioClip pickupSound;
        public AudioClip useSound;
        private PhotonView view;
        private bool isTaken = false;

        void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
        }

        public void PickupCrowbar(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("Crowbar");
            photonView.RPC("PlayPickupSound", RpcTarget.All);
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            StartCoroutine(PlaySoundAndDestroy());
        }

        private IEnumerator PlaySoundAndDestroy()
        {
            audioSource.PlayOneShot(pickupSound, 0.05f);
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
        }

        void Update()
        {
            if (photonView.IsMine)
            {
                if (ownerTransform != null)
                {
                    transform.position = ownerTransform.position + ownerTransform.forward * 0.1f + ownerTransform.right * 0.14f + Vector3.up * 0.3f;
                    transform.rotation = Quaternion.Euler(-110f, ownerTransform.eulerAngles.y, 0f);
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10);
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

        public void ShowCrowbar(bool show)
        {
            isTaken = show;

            if (!show)
            {
                photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
            }
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
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
