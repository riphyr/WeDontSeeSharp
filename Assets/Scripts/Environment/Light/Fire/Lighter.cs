﻿using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Lighter : MonoBehaviourPun, IPunObservable
    {
        public AudioClip pickupSound;
        private AudioSource audioSource;
        private PhotonView view;
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        private bool isDropped = false;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();

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

        public void PickupLighter(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("Lighter", 1);
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

        public void DropLighter()
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
    }
}
