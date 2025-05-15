using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Battery : MonoBehaviourPun, IPunObservable
    {
        public float batteryCharge = 100f;
        public AudioClip pickupSound;
        private AudioSource audioSource;
        private PhotonView view;
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        private bool isDropped = false;

        private Rigidbody rb;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
            rb = GetComponent<Rigidbody>();

            if (photonView.IsMine)
            {
                photonView.RPC("RPC_SyncPosition", RpcTarget.OthersBuffered, transform.position, transform.rotation);
            }
        }

        [PunRPC]
        private void RPC_SyncPosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            networkPosition = position;
            networkRotation = rotation;
        }

        public void PickupBattery(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("Battery", (int)batteryCharge);
            photonView.RPC("PlayPickupSound", RpcTarget.All);
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            audioSource.PlayOneShot(pickupSound);
        }

        [PunRPC]
        private void DestroyForAll()
        {
            Destroy(gameObject);
        }

        public void DropBattery()
        {
            if (!photonView.IsMine) return;

            isDropped = true;
            photonView.RPC("RPC_EnablePhysics", RpcTarget.All);
        }

        [PunRPC]
        public void RPC_EnablePhysics()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogWarning("Battery: Rigidbody manquant !");
                return;
            }

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        void Update()
        {
            if (!photonView.IsMine && isDropped)
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(isDropped);
            }
            else
            {
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
                isDropped = (bool)stream.ReceiveNext();
            }
        }
    }
}
