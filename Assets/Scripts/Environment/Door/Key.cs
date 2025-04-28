using UnityEngine;
using Photon.Pun;
<<<<<<< HEAD
using System.Collections;
=======
>>>>>>> emmarucay-patch-1

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
<<<<<<< HEAD
    public class Key : MonoBehaviourPun, IPunObservable
=======
    public class Key : MonoBehaviourPun
>>>>>>> emmarucay-patch-1
    {
        [Header("Paramètres de la clé")]
        public string keyName = "DefaultKey";

        [Header("Audio")] 
        public AudioSource audioSource;
        public AudioClip keyPickupSound;
<<<<<<< HEAD

        private PhotonView view;
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        private bool isDropped = false;
=======
        
        private PhotonView view;
>>>>>>> emmarucay-patch-1

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
<<<<<<< HEAD

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

        public void PickupKey(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem(keyName, 1);
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            photonView.RPC("PlayPickupSound", RpcTarget.All);
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            StartCoroutine(PlaySoundAndDestroy());
        }

        private IEnumerator PlaySoundAndDestroy()
        {
            audioSource.PlayOneShot(keyPickupSound);
            yield return new WaitForSeconds(keyPickupSound.length);
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DestroyForAll()
        {
            Destroy(gameObject);
        }

        public void DropKey()
        {
            if (!photonView.IsMine) return;

            isDropped = true;
            photonView.RPC("RPC_EnablePhysics", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_EnablePhysics()
        {
            isDropped = true;
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
=======
        }
        
        public void PickupKey(PlayerInventory inventory)
        {
            if (view.IsMine)
            {
                inventory.AddItem(keyName, 1);
                audioSource.PlayOneShot(keyPickupSound, 1.0f);
                view.RPC("DisableKeyForAll", RpcTarget.AllBuffered);
            }
            else
            {
                view.RequestOwnership();
            }
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
>>>>>>> emmarucay-patch-1
    }
}