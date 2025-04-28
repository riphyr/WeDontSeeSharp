using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Door : MonoBehaviour, IPunObservable
    {
        public enum DoorType { Normal, LockKey, PadLock }

        [Header("Type de porte")]
        public DoorType doorType = DoorType.Normal;

<<<<<<< HEAD
        [Header("Light blocker")]
        public GameObject lightBlocker;

=======
>>>>>>> emmarucay-patch-1
        private bool open;
        private float smooth = 1.0f;
        private float DoorOpenAngle = -90.0f;
        private float DoorCloseAngle = 0.0f;

        [Header("Audio")]
        private AudioSource asource;
        public AudioClip openDoor, closeDoor;
        [SerializeField] private AudioClip blockedDoorSound;

        private PhotonView view;

        void Start()
        {
            asource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
<<<<<<< HEAD
            UpdateLightBlocker();
=======
>>>>>>> emmarucay-patch-1
        }

        void Update()
        {
            if (open)
            {
<<<<<<< HEAD
                var target = Quaternion.Euler(0, DoorOpenAngle, 0);
=======
                var target = Quaternion.Euler (0, DoorOpenAngle, 0);
>>>>>>> emmarucay-patch-1
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * 5 * smooth);
            }
            else
            {
<<<<<<< HEAD
                var target1 = Quaternion.Euler(0, DoorCloseAngle, 0);
=======
                var target1= Quaternion.Euler (0, DoorCloseAngle, 0);
>>>>>>> emmarucay-patch-1
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target1, Time.deltaTime * 5 * smooth);
            }
        }

        public void ToggleDoor()
        {
<<<<<<< HEAD
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            if (doorType != DoorType.Normal && (HasActiveLock<LockKey>() || HasActiveLock<PadLock>()))
            {
                asource.PlayOneShot(blockedDoorSound, 1.0f);
                return;
            }

            open = !open;
            asource.clip = open ? openDoor : closeDoor;
            asource.Play();

            UpdateLightBlocker();
        }

        private void UpdateLightBlocker()
        {
            if (lightBlocker != null)
            {
                lightBlocker.SetActive(!open);
=======
            if (view.IsMine)
            {
                if (doorType != DoorType.Normal && (HasActiveLock<LockKey>() || HasActiveLock<PadLock>()))
                {
                    asource.PlayOneShot(blockedDoorSound, 1.0f);
                    return;
                }

                open = !open;
                asource.clip = open ? openDoor : closeDoor;
                asource.Play();
>>>>>>> emmarucay-patch-1
            }
        }

        private bool HasActiveLock<T>() where T : MonoBehaviour
        {
            T lockComponent = GetComponentInChildren<T>();
            return lockComponent != null && lockComponent.gameObject.activeSelf;
        }

        public bool IsOpen()
        {
            return open;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(open);
            }
            else
            {
                open = (bool)stream.ReceiveNext();
<<<<<<< HEAD
                UpdateLightBlocker();
            }
        }

=======
            }
        }
        
>>>>>>> emmarucay-patch-1
        void OnValidate()
        {
            if (doorType == DoorType.Normal)
            {
                blockedDoorSound = null;
            }
        }
    }
}
