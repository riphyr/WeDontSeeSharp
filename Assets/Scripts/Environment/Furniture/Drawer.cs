﻿using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Drawer : MonoBehaviourPun, IPunObservable
    {
        private bool isOpen = false;

        [Header("Déplacement du tiroir")]
        public float moveX = 0f;
        public float moveZ = 0f;
        private float speed = 2.0f;

        private Vector3 closedPosition;
        private Vector3 openPosition;

        public AudioSource audioSource;
        public AudioClip drawerOpen, drawerClose;
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();

            closedPosition = transform.localPosition;
            openPosition = closedPosition + new Vector3(moveX, 0, moveZ);
        }

        public void ToggleDrawer()
        {
            photonView.RPC("ToggleDrawer_RPC", RpcTarget.AllBuffered);
        }
        
        [PunRPC]
        private void ToggleDrawer_RPC()
        {
            isOpen = !isOpen;
            StopAllCoroutines();
            StartCoroutine(MoveDrawer(isOpen ? openPosition : closedPosition));

            audioSource.PlayOneShot(isOpen ? drawerOpen : drawerClose, isOpen ? 0.1f : 0.05f);
        }

        private IEnumerator MoveDrawer(Vector3 targetPosition)
        {
            while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * speed);
                yield return null;
            }

            transform.localPosition = targetPosition;
        }
        
        public bool IsOpen()
        {
            return isOpen;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isOpen);
            }
            else
            {
                bool previousState = isOpen;
                isOpen = (bool)stream.ReceiveNext();

                if (isOpen != previousState)
                {
                    StopAllCoroutines(); 
                    StartCoroutine(MoveDrawer(isOpen ? openPosition : closedPosition));
                }
            }
        }
    }
}
