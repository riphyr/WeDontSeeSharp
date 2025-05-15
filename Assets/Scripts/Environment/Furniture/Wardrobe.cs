using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Wardrobe : MonoBehaviourPun, IPunObservable
    {
        public enum OpeningMode { Rotation, Slide }

        [Header("Mode d’ouverture")]
        public OpeningMode openingMode = OpeningMode.Rotation;

        [Header("Paramètres de rotation")]
        public bool isLeftDoor = true;
        public float rotationAngle = 90f;

        [Header("Paramètres de slide")]
        public float moveX = 0f;
        public float moveY = 0f;
        public float moveZ = 0f;

        private float speed = 2.0f;
        private bool isOpen = false;

        private Quaternion closedRotation;
        private Quaternion openRotation;
        private Vector3 closedPosition;
        private Vector3 openPosition;

        [Header("Audio")]
        private AudioSource audioSource;
        public AudioClip wardrobeOpen, wardrobeClose;
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
            view.OwnershipTransfer = OwnershipOption.Takeover;

            if (openingMode == OpeningMode.Rotation)
            {
                float angle = isLeftDoor ? rotationAngle : -rotationAngle;
                closedRotation = transform.localRotation;
                openRotation = Quaternion.Euler(transform.localRotation.eulerAngles + new Vector3(0, angle, 0));
            }
            else
            {
                closedPosition = transform.localPosition;
                openPosition = closedPosition + new Vector3(moveX, moveY, moveZ);
            }
        }

        public void ToggleWardrobe()
        {
            if (!view.IsMine)
                view.TransferOwnership(PhotonNetwork.LocalPlayer);

            isOpen = !isOpen;
            StopAllCoroutines();

            if (openingMode == OpeningMode.Rotation)
                StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
            else
                StartCoroutine(MoveDoor(isOpen ? openPosition : closedPosition));

            if (view.IsMine)
                audioSource.PlayOneShot(isOpen ? wardrobeOpen : wardrobeClose, 0.1f);
        }

        private IEnumerator RotateDoor(Quaternion targetRotation)
        {
            while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * speed);
                yield return null;
            }

            transform.localRotation = targetRotation;
        }

        private IEnumerator MoveDoor(Vector3 targetPosition)
        {
            while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * speed);
                yield return null;
            }

            transform.localPosition = targetPosition;
        }

        public bool IsOpen() => isOpen;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
                stream.SendNext(isOpen);
            else
            {
                bool prev = isOpen;
                isOpen = (bool)stream.ReceiveNext();

                if (isOpen != prev)
                {
                    StopAllCoroutines();

                    if (openingMode == OpeningMode.Rotation)
                        StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
                    else
                        StartCoroutine(MoveDoor(isOpen ? openPosition : closedPosition));
                }
            }
        }
    }
}
