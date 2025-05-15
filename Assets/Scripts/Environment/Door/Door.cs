using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine.AI;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Door : MonoBehaviour, IPunObservable
    {
        public enum DoorType { Normal, LockKey, PadLock, Crowbar }

        [Header("Type de porte")]
        public DoorType doorType = DoorType.Normal;

        public bool Teleport;
        public GameObject teleport;
        public TMP_Text blockedMessage;
        
        [Header("Light blocker")]
        public GameObject lightBlocker;

        private bool open;
        private float smooth = 1.0f;
        private float DoorOpenAngle = -90.0f;
        private float DoorCloseAngle = 0.0f;

        [Header("Audio")]
        private AudioSource asource;
        public AudioClip openDoor, closeDoor;
        [SerializeField] private AudioClip blockedDoorSound;
        [SerializeField] private AudioClip forcedDoorSound;

        private PhotonView view;
        private NavMeshObstacle navObstacle;
        private NavMeshLink navLink;

        void Start()
        {
            asource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
            
            if (transform.parent != null && transform.parent.parent != null)
            {
                navObstacle = transform.parent.parent.GetComponent<NavMeshObstacle>();
                navLink = transform.parent.parent.GetComponent<NavMeshLink>();
            }
            
            UpdateLightBlocker();
            UpdateObstacle();
            
        }

        void Update()
        {
            if (open)
            {
                var target = Quaternion.Euler (0, DoorOpenAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * 5 * smooth);
            }
            else
            {
                var target1= Quaternion.Euler (0, DoorCloseAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target1, Time.deltaTime * 5 * smooth);
            }
        }

        public void ToggleDoor(bool hasCrowbarEquipped = false)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
            if (view.IsMine)
            {
                var doorTeleport = teleport?.GetComponent<DoorTeleport>();

                // Vérifie avec GameProgressManager si la porte précédente est validée
                if (Teleport && doorTeleport != null && !doorTeleport.IsPreviousDoorDone())
                {
                    asource.PlayOneShot(blockedDoorSound, 1.0f);
                    return;
                }

                if (doorType != DoorType.Normal && (HasActiveLock<LockKey>() || HasActiveLock<PadLock>()))
                {
                    asource.PlayOneShot(blockedDoorSound, 1.0f);
                    return;
                }
                
                if (doorType == DoorType.Crowbar)
                {
                    if (!open)
                    {
                        if (hasCrowbarEquipped)
                        {
                            open = true;
                            asource.PlayOneShot(forcedDoorSound, 0.6f);
                            UpdateLightBlocker();
                            UpdateObstacle();
                        }
                        else
                        {
                            asource.PlayOneShot(blockedDoorSound, 1.0f);
                        }
                    }
                    else
                    {
                    
                    }

                    return;
                }

                open = !open;
                asource.clip = open ? openDoor : closeDoor;
                asource.Play();
                UpdateLightBlocker();
                UpdateObstacle();

                if (Teleport && open)
                {
                    if (doorTeleport != null)
                        doorTeleport.OnDoorOpened();
                }
                else
                {
                    if (doorTeleport != null)
                        doorTeleport.OnDoorClosed();
                }
                
            }
        }
        
        private void UpdateLightBlocker()
        {
            if (lightBlocker != null)
            {
                lightBlocker.SetActive(!open);
            }
        }
        
        private void UpdateObstacle()
        {
            if (navObstacle != null)
            {
                navObstacle.carving = !open;
            }
            if (navLink != null)
            {
                navLink.enabled = open;
            }

        }


        public bool HasActiveLock<T>() where T : MonoBehaviour
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
            }
        }
        
        void OnValidate()
        {
            if (doorType == DoorType.Normal)
            {
                blockedDoorSound = null;
            }
        }
        
       
    }
}
