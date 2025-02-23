
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;
using System.Collections;

namespace EnivStudios
{
    public class DoorController : MonoBehaviour
    {
        public enum states {NormalDoor,HoldKeyDoor,KeyDoor,KeypadDoor,PadLockDoor,LeverDoor,KeycardDoor}
        [SerializeField] states doorType;    

        public enum anims { NoAnimation,KeyAnimation,LockKeyAnimation}
        [SerializeField] anims animationType;

        public enum key {BlueKey,RedKey,GreenKey,BlackKey}
        public key unlockKey;

        public enum keycards { BlueKeycard, RedKeycard, GreenKeycard, BlackKeycard}
        public keycards unlockKeycard;

        public enum DoorAxis { XAxis,YAxis,ZAxis}
        [SerializeField] DoorAxis doorForwardAxis;

        [SerializeField] bool oneWay;
        [SerializeField] bool twoWay;
        [Range(1,10)] public float doorRotationSpeed = 4.5f;
        [SerializeField] bool autoClose;
        [SerializeField] float autoCloseTimer = 5f;
        [SerializeField] AudioClip doorOpenSound, doorCloseSound;
        public string openDoorUI,closeDoorUI,lockedDoorUI,useKeyUI;
        public AudioClip doorLockSound, useKeySound;
        public string doorLockedText;
        public float textDisplayTimer;
        [SerializeField] Transform door;
        [SerializeField] DoorUI doorUI;
        [SerializeField] float targetDoor = -90f;
        [SerializeField] GameObject keyPrefab;
        [SerializeField] GameObject lockModel;
        public AnimationClip keyAnim;
        [SerializeField] bool inverseDoorRotationX, inverseDoorRotationY, inverseDoorRotationZ;
        public float circleTimerr = 1.7f;

        //Private Variables
        float targetDoorRotation;
        float defaultDoorRotation;
        Transform playerPosition;
        [HideInInspector] public AudioSource audioSource;

        [HideInInspector] public bool isOpen;
        float timer;
        [HideInInspector] public bool normalDoor, keyDoor, holdKeyDoor, holdDoor, waitTime,keypadDoor,padlockDoor,LeverDoor,keycardDoor;
        [HideInInspector] public bool noAnimation, keyAnimation, lockKeyAnimation,keycardAnimation,keyDoorAnim;
        [HideInInspector] public float circleTimer;
        private void Start()
        {
            doorUI = FindObjectOfType<DoorUI>();
            audioSource = GetComponent<AudioSource>();
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform;
            defaultDoorRotation = transform.eulerAngles.y;
            if (doorType == states.NormalDoor)
            {
                normalDoor = true;
            }

            if(doorType == states.KeyDoor)
            {
                keyDoor = true;
            }
            if(doorType == states.HoldKeyDoor)
            {
                holdKeyDoor = true;
            }
            
            if(doorType == states.KeypadDoor)
            {
                keypadDoor = true;
            }
            if(doorType == states.PadLockDoor)
            {
                padlockDoor = true;
            }
            if(doorType == states.LeverDoor)
            {
                LeverDoor = true;
            }
            if(doorType == states.KeycardDoor)
            {
                keycardDoor = true;
            }
            if(animationType == anims.NoAnimation)
            {
                noAnimation = true;
            }
            if (animationType == anims.KeyAnimation)
            {
                keyAnimation = true;
            }
            if (animationType == anims.LockKeyAnimation)
            {
                lockKeyAnimation = true;
            } 
        }
        private void Update()
        { 
            door.rotation = Quaternion.Lerp(door.rotation, Quaternion.Euler(0f, defaultDoorRotation + targetDoorRotation, 0f), doorRotationSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            if (timer <= 0f && isOpen && autoClose)
            {
                audioSource.clip = doorCloseSound;
                audioSource.Play();
                ToggleDoor(playerPosition.position);
            }
        }
        void ToggleDoor(Vector3 position)
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                doorUI.doorState = true;
                Vector3 dir = position - transform.position;
                if (twoWay) 
                {
                    if (doorForwardAxis == DoorAxis.XAxis)
                    {
                        if (!inverseDoorRotationX)
                        {
                            targetDoorRotation = Mathf.Sign(Vector3.Dot(transform.right, dir)) * 90f;
                        }
                        else
                        {
                            targetDoorRotation = -Mathf.Sign(Vector3.Dot(transform.right, dir)) * 90f;
                        }
                    }
                    else if (doorForwardAxis == DoorAxis.YAxis)
                    {
                        if (!inverseDoorRotationY)
                        {
                            targetDoorRotation = -Mathf.Sign(Vector3.Dot(transform.up, dir)) * 90f;
                        }
                        else
                        {
                            targetDoorRotation = Mathf.Sign(Vector3.Dot(transform.up, dir)) * 90f;
                        }
                    }
                    else if (doorForwardAxis == DoorAxis.ZAxis)
                    {
                        if (!inverseDoorRotationZ)
                        {
                            targetDoorRotation = -Mathf.Sign(Vector3.Dot(transform.forward, dir)) * 90f;
                        }
                        else
                        {
                            targetDoorRotation = Mathf.Sign(Vector3.Dot(transform.forward, dir)) * 90f;
                        }
                    }
                }
                else
                {
                    targetDoorRotation = targetDoor;
                }
                timer = autoCloseTimer;
            }
            else
            {
                doorUI.doorState = false;
                targetDoorRotation = 0f;
            }
        }
        public void DoorInteract()
        {
            ToggleDoor(playerPosition.position);
            if (!isOpen)
            {
                audioSource.clip = doorOpenSound;
                audioSource.Play();
            }
            else
            {
                audioSource.clip = doorCloseSound;
                audioSource.Play();
            }
        }
        public IEnumerator DoorOpen()
        {
            waitTime = true;
            holdDoor = false;
            DoorInteract();
            yield return new WaitForSeconds(1);
            waitTime = false;
        }
        public void DoorLockedText()
        {
            doorUI.doorLockedText.text = doorLockedText;
        }
    }
}
