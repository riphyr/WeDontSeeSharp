using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Closet : MonoBehaviourPun, IPunObservable
    {
        private bool isOpen = false;

        [Header("Paramètres de rotation")]
        public bool isLeftDoor = true;
        public float rotationAngle = 90f;
        private float speed = 2.0f;

        private Quaternion closedRotation;
        private Quaternion openRotation;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip closetOpen, closetClose;
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();

            float angle = isLeftDoor ? rotationAngle : -rotationAngle;
            closedRotation = transform.localRotation;
            openRotation = Quaternion.Euler(transform.localRotation.eulerAngles + new Vector3(0, angle, 0));
        }

        public void ToggleCloset()
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
            
            isOpen = !isOpen;
            StopAllCoroutines();
            StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
        }

        private IEnumerator RotateDoor(Quaternion targetRotation)
        {
            if (view.IsMine)
            {
                audioSource.PlayOneShot(isOpen ? closetOpen : closetClose, 0.1f);
            }

            while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * speed);
                yield return null;
            }

            transform.localRotation = targetRotation;
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
                    StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
                }
            }
        }
    }
}
