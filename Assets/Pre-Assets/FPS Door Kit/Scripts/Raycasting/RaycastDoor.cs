
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

namespace EnivStudios
{
    public class RaycastDoor : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField] [Range(1, 10)] float rayDistance = 2f;
        [SerializeField] LayerMask layerMask;

        [Header("Examine Properties")]
        [SerializeField] string examineLayerName;
        [SerializeField] Transform examineItemPos;
        [SerializeField] GameObject examineCam;

        [Space(2)]
        [SerializeField] bool padLockDoor;
        [EnivInspector("padLockDoor", false)] [SerializeField] Transform padlockPos;

        [Header("Post Processing Effects")]
        [SerializeField] PostProcessProfile examineBlur;
        [SerializeField] PostProcessProfile defaultProfile;
        PostProcessVolume myVolume;

        //Scripts
        [Header("Scripts")]
        public PlayerMovement playerMovement;
        DoorUI doorUI;

        DoorController doorController;
        LockController lockController;
        LockOpener lockOpener;

        //Private Variables
        bool examining = false;
        bool time;

        Vector3 originalPos;
        Quaternion originalRot;

        GameObject inspected;
        GameObject currentlyFocused;

        int previousLayer;
        float originalFOV;
        float examineOriginalFOV;
        float timer;

        Camera _cam;

        [HideInInspector] public bool antiAnimationRepeat;

        private void Start()
        {
            myVolume = GetComponent<PostProcessVolume>();
            doorUI = FindObjectOfType<DoorUI>();
            doorUI.doorKeyCode.text = doorUI.interactKey.ToString();
            doorUI.holdDoorKeyCode.text = doorUI.interactKey.ToString();
            doorUI.InteractKeyCode.text = doorUI.interactKey.ToString();
            doorUI.examineKeyCode.text = doorUI.examineKey.ToString();
            _cam = GetComponent<Camera>();
            originalFOV = _cam.fieldOfView;
            if (examineCam != null)
            {
                examineOriginalFOV = examineCam.GetComponent<Camera>().fieldOfView;
            }
        }
        private void Update()
        {
            bool crosshairChangeColor = false;
            bool interact = Input.GetKeyDown(doorUI.interactKey);
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, rayDistance, layerMask.value | (1 << LayerMask.NameToLayer(examineLayerName)))) 
            {
                // Door Controller
                if (doorController = hit.collider.GetComponent<DoorController>())
                {
                    #region Door Types
                    if (doorController.normalDoor)
                    {
                        #region Normal Door
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { doorUI.crosshair.color = doorUI.unlockCrosshairColor; doorUI.doorKeyBG.SetActive(true); }
                        if (interact)
                        {
                            doorController.DoorInteract();
                        }
                        DoorInteraction();
                        #endregion
                    }
                    else if (doorController.holdKeyDoor)
                    {
                        #region HoldKey Door
                        crosshairChangeColor = true;
                        if (crosshairChangeColor)
                        {
                            if (!doorController.isOpen) { doorUI.holdDoorSateText.text = doorController.openDoorUI; }
                            doorUI.crosshair.color = doorUI.unlockCrosshairColor; doorUI.holdDoorKeyBG.SetActive(true);
                        }
                        if (Input.GetKey(doorUI.interactKey) && doorController.circleTimer < doorController.circleTimerr && !doorController.waitTime)
                        {
                            doorController.circleTimer += Time.deltaTime;
                            float time = doorController.circleTimer / doorController.circleTimerr;
                            doorUI.backgroundCircle.fillAmount = time;
                            if (doorController.circleTimer > doorController.circleTimerr) { doorController.holdDoor = true; }
                        }
                        else if (doorController.holdDoor) 
                        {
                            if (!doorController.isOpen) { doorUI.holdDoorSateText.text = doorController.closeDoorUI; }
                            else { doorUI.holdDoorSateText.text = doorController.openDoorUI; }
                            StartCoroutine(doorController.DoorOpen());
                        }
                        else
                        {
                            doorController.circleTimer = 0;
                            float time = doorController.circleTimer / doorController.circleTimerr;
                            doorUI.backgroundCircle.fillAmount = time;
                        }
                        #endregion
                    }
                    else if (doorController.keyDoor)
                    {
                        #region Key Door
                        if (doorController.noAnimation)
                        {
                            bool blue = !doorUI.blueKey && doorController.unlockKey == DoorController.key.BlueKey, red = !doorUI.redKey && doorController.unlockKey == DoorController.key.RedKey;
                            bool green = !doorUI.greenKey && doorController.unlockKey == DoorController.key.GreenKey, black = !doorUI.blackKey && doorController.unlockKey == DoorController.key.BlackKey;
                            crosshairChangeColor = true;
                            if (crosshairChangeColor && blue || crosshairChangeColor && red || crosshairChangeColor && green || crosshairChangeColor && black)
                            {
                                InteractAndExamine();
                                if (interact)
                                {
                                    DoorLockSound();
                                }
                            }
                            else if (crosshairChangeColor && doorController.unlockKey == DoorController.key.BlueKey)
                            {
                                DoorAndExamine();
                                if (interact && doorUI.blueKey && !doorUI.bKey)
                                {
                                    HideInfo();
                                    StartCoroutine(DoorUnlock());
                                }
                                else if (interact && doorUI.bKey) { doorController.DoorInteract(); }
                                if (doorUI.blueKey && !doorUI.bKey) { doorUI.doorsSateText.text = doorController.useKeyUI; }
                                else { DoorInteraction(); }
                            }
                            else if (crosshairChangeColor && doorController.unlockKey == DoorController.key.RedKey)
                            {
                                DoorAndExamine();
                                if (interact && doorUI.redKey && !doorUI.rKey)
                                {
                                    HideInfo();
                                    StartCoroutine(DoorUnlock());
                                }
                                else if (interact && doorUI.rKey) { doorController.DoorInteract(); }
                                if (doorUI.redKey && !doorUI.rKey) { doorUI.doorsSateText.text = doorController.useKeyUI; }
                                else { DoorInteraction(); }
                            }
                            else if (crosshairChangeColor && doorController.unlockKey == DoorController.key.GreenKey)
                            {
                                DoorAndExamine();
                                if (interact && doorUI.greenKey && !doorUI.gKey)
                                {
                                    HideInfo();
                                    StartCoroutine(DoorUnlock());
                                }
                                else if (interact && doorUI.gKey) { doorController.DoorInteract(); }
                                if (doorUI.greenKey && !doorUI.gKey) { doorUI.doorsSateText.text = doorController.useKeyUI; }
                                else { DoorInteraction(); }
                            }
                            else if (crosshairChangeColor && doorController.unlockKey == DoorController.key.BlackKey)
                            {
                                DoorAndExamine();
                                if (interact && doorUI.blackKey && !doorUI.bbKey)
                                {
                                    HideInfo();
                                    StartCoroutine(DoorUnlock());
                                }
                                else if (interact && doorUI.bbKey) { doorController.DoorInteract(); }
                                if (doorUI.blackKey && !doorUI.bbKey) { doorUI.doorsSateText.text = doorController.useKeyUI; }
                                else { DoorInteraction(); }
                            }
                        }
                        else if (doorController.keyAnimation)
                        {
                            bool blue = !doorUI.blueKey && doorController.unlockKey == DoorController.key.BlueKey, red = !doorUI.redKey && doorController.unlockKey == DoorController.key.RedKey;
                            bool green = !doorUI.greenKey && doorController.unlockKey == DoorController.key.GreenKey, black = !doorUI.blackKey && doorController.unlockKey == DoorController.key.BlackKey;
                            crosshairChangeColor = true;
                            if (crosshairChangeColor && blue || crosshairChangeColor && red || crosshairChangeColor && green || crosshairChangeColor && black)
                            {
                                InteractAndExamine();
                                if (interact)
                                {
                                    DoorLockSound();
                                }
                            }
                            else if (crosshairChangeColor && doorController.unlockKey == DoorController.key.BlueKey)
                            {
                                DoorAndExamine();
                                if (interact && doorUI.blueKey && !doorUI.bkKey && !doorController.keyDoorAnim)
                                {
                                    doorController.keyDoorAnim = true;
                                    HideInfo();
                                    StartCoroutine(DoorUnlockk());
                                }
                                else if (interact && doorUI.bkKey) { doorController.DoorInteract(); }
                                if (doorUI.blueKey && !doorUI.bkKey) { doorUI.doorsSateText.text = doorController.useKeyUI; }
                                else { DoorInteraction(); }
                            }
                            else if (crosshairChangeColor && doorController.unlockKey == DoorController.key.RedKey)
                            {
                                DoorAndExamine();
                                if (interact && doorUI.redKey && !doorUI.rrKey && !doorController.keyDoorAnim)
                                {
                                    doorController.keyDoorAnim = true;
                                    HideInfo();
                                    StartCoroutine(DoorUnlockk());
                                }
                                else if (interact && doorUI.rrKey) { doorController.DoorInteract(); }
                                if (doorUI.redKey && !doorUI.rrKey) { doorUI.doorsSateText.text = doorController.useKeyUI; }
                                else { DoorInteraction(); }
                            }
                            else if (crosshairChangeColor && doorController.unlockKey == DoorController.key.GreenKey)
                            {
                                DoorAndExamine();
                                if (interact && doorUI.greenKey && !doorUI.ggKey && !doorController.keyDoorAnim)
                                {
                                    doorController.keyDoorAnim = true;
                                    HideInfo();
                                    StartCoroutine(DoorUnlockk());
                                }
                                else if (interact && doorUI.ggKey) { doorController.DoorInteract(); }
                                if (doorUI.greenKey && !doorUI.ggKey) { doorUI.doorsSateText.text = doorController.useKeyUI; }
                                else { DoorInteraction(); }
                            }
                            else if (crosshairChangeColor && doorController.unlockKey == DoorController.key.BlackKey)
                            {
                                DoorAndExamine();
                                if (interact && doorUI.blackKey && !doorUI.blKey && !doorController.keyDoorAnim)
                                {
                                    doorController.keyDoorAnim = true;
                                    HideInfo();
                                    StartCoroutine(DoorUnlockk());
                                }
                                else if (interact && doorUI.blKey) { doorController.DoorInteract(); }
                                if (doorUI.blackKey && !doorUI.blKey) { doorUI.doorsSateText.text = doorController.useKeyUI; }
                                else { DoorInteraction(); }
                            }
                        }
                        else if (doorController.lockKeyAnimation)
                        {
                            bool blue = !doorUI.blueDoorOpened && doorController.unlockKey == DoorController.key.BlueKey, red = !doorUI.redDoorOpened && doorController.unlockKey == DoorController.key.RedKey;
                            bool green = !doorUI.greenDoorOpened && doorController.unlockKey == DoorController.key.GreenKey, black = !doorUI.blackDoorOpened && doorController.unlockKey == DoorController.key.BlackKey;
                            crosshairChangeColor = true;
                            if (crosshairChangeColor && blue || crosshairChangeColor && red || crosshairChangeColor && green || crosshairChangeColor && black)
                            {
                                InteractAndExamine();
                                if (interact)
                                {
                                    DoorLockSound();
                                }
                            }
                            else if (crosshairChangeColor && doorUI.blueDoorOpened && doorController.unlockKey == DoorController.key.BlueKey)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.redDoorOpened && doorController.unlockKey == DoorController.key.RedKey)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.greenDoorOpened && doorController.unlockKey == DoorController.key.GreenKey)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.blackDoorOpened && doorController.unlockKey == DoorController.key.BlackKey)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                        }
                        #endregion
                    }
                    else if (doorController.keypadDoor)
                    {
                        #region Keypad Door
                        crosshairChangeColor = true;
                        if (crosshairChangeColor && !doorUI.keypadDoorOpned)
                        {
                            doorUI.crosshair.color = doorUI.lockCrosshairColor; doorUI.InteractText.text = doorController.lockedDoorUI; doorUI.InteractKeyBG.SetActive(true);
                            if (interact)
                            {
                                DoorLockSound();
                            }
                        }
                        else if (crosshairChangeColor && doorUI.keypadDoorOpned)
                        {
                            doorUI.crosshair.color = doorUI.unlockCrosshairColor; doorUI.doorKeyBG.SetActive(true);
                            if (interact)
                            {
                                doorController.DoorInteract();
                            }
                            DoorInteraction();
                        }
                        #endregion
                    }
                    else if (doorController.padlockDoor)
                    {
                        #region Padlock Door
                        crosshairChangeColor = true;
                        if (crosshairChangeColor && !doorUI.padlockDoorOpened)
                        {
                            InteractAndExamine();
                            if (interact)
                            {
                                DoorLockSound();
                            }
                        }
                        else if (crosshairChangeColor && doorUI.padlockDoorOpened)
                        {
                            doorUI.crosshair.color = doorUI.unlockCrosshairColor; doorUI.doorKeyBG.SetActive(true);
                            if (interact)
                            {
                                doorController.DoorInteract();
                            }
                            DoorInteraction();
                        }
                        #endregion
                    }
                    else if (doorController.keycardDoor)
                    {
                        #region Keycard Door
                        if (doorController.keycardAnimation)
                        {
                            bool blue = !doorUI.blueKeycardDoorOpened && doorController.unlockKeycard == DoorController.keycards.BlueKeycard,
                            red = !doorUI.redKeycardDoorOpened && doorController.unlockKeycard == DoorController.keycards.RedKeycard;
                            bool green = !doorUI.greenKeycardDoorOpened && doorController.unlockKeycard == DoorController.keycards.GreenKeycard,
                            black = !doorUI.blackKeycardDoorOpened && doorController.unlockKeycard == DoorController.keycards.BlackKeycard;
                            crosshairChangeColor = true;
                            if (crosshairChangeColor && blue || crosshairChangeColor && red || crosshairChangeColor && green || crosshairChangeColor && black)
                            {
                                InteractAndExamine();
                                if (interact)
                                {
                                    DoorLockSound();
                                }
                            }
                            if (crosshairChangeColor && doorUI.blueKeycardDoorOpened && doorController.unlockKeycard == DoorController.keycards.BlueKeycard)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.redKeycardDoorOpened && doorController.unlockKeycard == DoorController.keycards.RedKeycard)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.greenKeycardDoorOpened && doorController.unlockKeycard == DoorController.keycards.GreenKeycard)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.blackKeycardDoorOpened && doorController.unlockKeycard == DoorController.keycards.BlackKeycard)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                        }
                        else if(!doorController.keycardAnimation)
                        {
                            bool blue =  doorController.unlockKeycard == DoorController.keycards.BlueKeycard && !doorUI.bdOpen,
                            red = doorController.unlockKeycard == DoorController.keycards.RedKeycard && !doorUI.rOpen;
                            bool green = doorController.unlockKeycard == DoorController.keycards.GreenKeycard && !doorUI.gOpen,
                            black = doorController.unlockKeycard == DoorController.keycards.BlackKeycard && !doorUI.bldOpen;
                            crosshairChangeColor = true;
                            if (crosshairChangeColor && blue || crosshairChangeColor && red || crosshairChangeColor && green || crosshairChangeColor && black)
                            {
                                InteractAndExamine();
                                if (interact)
                                {
                                    DoorLockSound();
                                }
                            }
                            if (crosshairChangeColor && doorUI.bdOpen && doorController.unlockKeycard == DoorController.keycards.BlueKeycard)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.gdOpen && doorController.unlockKeycard == DoorController.keycards.GreenKeycard)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.rdOpen && doorController.unlockKeycard == DoorController.keycards.RedKeycard)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                            else if (crosshairChangeColor && doorUI.bldOpen && doorController.unlockKeycard == DoorController.keycards.BlackKeycard)
                            {
                                DoorAndExamine();
                                if (interact)
                                {
                                    HideInfo();
                                    doorController.DoorInteract();
                                }
                                DoorInteraction();
                            }
                        }
                        #endregion
                    }   
                    #endregion
                }
                // Lock Controller
                else if (lockController = hit.collider.GetComponent<LockController>())
                {
                    #region Lock Types
                    if (lockController.lockType == LockController.locks.NormalLock)
                    {
                        #region KeyLock
                        crosshairChangeColor = true;
                        if (crosshairChangeColor && !antiAnimationRepeat)
                        {
                            ExamineAndInteract();
                        }
                        if (!examining)
                        {
                            if (Input.GetKeyDown(doorUI.examineKey) && !doorUI.startAgain)
                            {
                                doorUI.circleBG.SetActive(true);
                                doorUI.mouseBG.SetActive(false);
                                Interacting();
                                doorUI.mainPutBackBG.GetComponent<RectTransform>().transform.position = new Vector3(doorUI.mainPutBackBG.transform.position.x, 181.1f, 0);
                                inspected = hit.transform.gameObject;
                                originalPos = hit.transform.position;
                                originalRot = hit.transform.rotation;
                                inspected.transform.SetParent(examineItemPos);
                            }
                        }
                        else if (Input.GetKeyDown(doorUI.interactKey) && examining && !antiAnimationRepeat)
                        {
                            if (doorUI.blueKey && lockController.normallockType == LockController.normallock.BlueKeyLock)
                            {
                                NormalLock();
                                hit.collider.gameObject.GetComponent<Animation>().clip = lockController.keyLockAnim;
                                hit.collider.gameObject.GetComponent<Animation>().Play();
                                doorUI.blueDoorOpened = true;
                            }
                            else if (doorUI.redKey && lockController.normallockType == LockController.normallock.RedKeyLock)
                            {
                                NormalLock();
                                hit.collider.gameObject.GetComponent<Animation>().clip = lockController.keyLockAnim;
                                hit.collider.gameObject.GetComponent<Animation>().Play();
                                doorUI.redDoorOpened = true;
                            }
                            else if (doorUI.greenKey && lockController.normallockType == LockController.normallock.GreenKeyLock)
                            {
                                NormalLock();
                                hit.collider.gameObject.GetComponent<Animation>().clip = lockController.keyLockAnim;
                                hit.collider.gameObject.GetComponent<Animation>().Play();
                                doorUI.greenDoorOpened = true;
                            }
                            else if (doorUI.blackKey && lockController.normallockType == LockController.normallock.BlackKeyLock)
                            {
                                NormalLock();
                                hit.collider.gameObject.GetComponent<Animation>().clip = lockController.keyLockAnim;
                                hit.collider.gameObject.GetComponent<Animation>().Play();
                                doorUI.blackDoorOpened = true;
                            }
                            else 
                            {
                                LockedExamine();
                            }

                        }
                        OnExamining();
                        #endregion
                    }
                    else if (lockController.lockType == LockController.locks.Keypad)
                    {
                        #region KeypadLock
                        lockController.keypadNo = lockController.keypadNumber;
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { doorUI.crosshair.color = doorUI.unlockCrosshairColor; doorUI.examineKeyBG.SetActive(true); }
                        if (!examining)
                        {
                            if (Input.GetKeyDown(doorUI.examineKey) && !doorUI.startAgain)
                            {
                                doorUI.circleBG.SetActive(false);
                                doorUI.mouseBG.SetActive(true);
                                Interacting();
                                doorUI.mainPutBackBG.GetComponent<RectTransform>().transform.position = new Vector3(doorUI.mainPutBackBG.transform.position.x, 70, 0);
                                Cursor.visible = true;
                                Cursor.lockState = CursorLockMode.None;
                                inspected = hit.transform.gameObject;
                                originalPos = hit.transform.position;
                                originalRot = hit.transform.rotation;
                                inspected.transform.SetParent(examineItemPos);
                            }
                        }
                        OnExamining();
                        #endregion
                    }
                    else if(lockController.lockType == LockController.locks.Padlock)
                    {
                        #region Padlock
                        crosshairChangeColor = true;
                        if (crosshairChangeColor && !antiAnimationRepeat)
                        {
                            ExamineAndInteract();
                        }
                        if (!examining)
                        {
                            if (Input.GetKeyDown(doorUI.examineKey) && !doorUI.startAgain)
                            {
                                examining = true;
                                doorUI.mainPutBackBG.SetActive(true);
                                doorUI.circleBG.SetActive(false);
                                doorUI.mouseBG.SetActive(true);
                                ExamineTextAnimations();
                                StartCoroutine(lockController.PadLockOpen());
                                doorUI.mainPutBackBG.GetComponent<RectTransform>().transform.position = new Vector3(doorUI.mainPutBackBG.transform.position.x, 70, 0);
                                Cursor.visible = true;
                                Cursor.lockState = CursorLockMode.None;
                                inspected = hit.transform.gameObject;
                                inspected = hit.collider.gameObject;
                                originalPos = hit.transform.position;
                                originalRot = hit.transform.rotation;
                                inspected.transform.SetParent(examineItemPos);
                            }
                        }
                        OnExamining();
                        #endregion
                    }
                   
                    else if(lockController.lockType == LockController.locks.CardReader)
                    {
                        #region KeyCard Lock
                        bool blue = lockController.keycard == LockController.keycards.BlueCardReader && doorUI.blueKeycardDoorOpened;
                        bool green = lockController.keycard == LockController.keycards.GreenCardReader && doorUI.greenKeycardDoorOpened;
                        bool black = lockController.keycard == LockController.keycards.BlackCardReader && doorUI.blackKeycardDoorOpened;
                        bool red = lockController.keycard == LockController.keycards.RedCardReader && doorUI.redKeycardDoorOpened;
                        crosshairChangeColor = true;

                        if (lockController.NoAnimation)
                        {
                            if (crosshairChangeColor)
                            {
                                doorUI.crosshair.color = doorUI.unlockCrosshairColor; doorUI.examineKeyBG.SetActive(false);
                                doorUI.InteractKeyBG.SetActive(true);
                            }
                            if(interact && doorUI.blueKeycard && lockController.keycard == LockController.keycards.BlueCardReader)
                            {
                                if (!doorUI.bOpen)
                                {
                                    lockController.audioSource.clip = lockController.rightCodeSound;
                                    lockController.audioSource.Play();
                                }
                                doorUI.bOpen = true;
                                doorUI.bdOpen = true;
                                lockController.KeycardAccept();
                            }
                            else if (interact && doorUI.greenKeyCard && lockController.keycard == LockController.keycards.GreenCardReader)
                            {
                                if (!doorUI.gOpen)
                                {
                                    lockController.audioSource.clip = lockController.rightCodeSound;
                                    lockController.audioSource.Play();
                                }
                                doorUI.gOpen = true;
                                doorUI.gdOpen = true;
                                lockController.KeycardAccept();
                            }
                            else if (interact && doorUI.redKeyCard && lockController.keycard == LockController.keycards.RedCardReader)
                            {
                                if (!doorUI.rOpen)
                                {
                                    lockController.audioSource.clip = lockController.rightCodeSound;
                                    lockController.audioSource.Play();
                                }
                                doorUI.rOpen = true;
                                doorUI.rdOpen = true;
                                lockController.KeycardAccept();
                            }
                            else if (interact && doorUI.blackKeyCard && lockController.keycard == LockController.keycards.BlackCardReader)
                            {
                                if (!doorUI.bbOpen)
                                {
                                    lockController.audioSource.clip = lockController.rightCodeSound;
                                    lockController.audioSource.Play();
                                }
                                doorUI.bbOpen = true;
                                doorUI.bldOpen = true;
                                lockController.KeycardAccept();
                            }
                            else if (interact)
                            {
                                lockController.audioSource.clip = lockController.wrongCodeSound;
                                lockController.audioSource.Play();
                                lockController.KeycardDecline();
                            }
                        }
                        else if (lockController.KeycardAnimation)
                        {

                            if (crosshairChangeColor)
                            {
                                ExamineAndInteract();
                            }
                            if (!examining)
                            {
                                if (Input.GetKeyDown(doorUI.examineKey) && !doorUI.startAgain)
                                {
                                    doorUI.circleBG.SetActive(true);
                                    doorUI.mouseBG.SetActive(false);
                                    doorUI.mainPutBackBG.GetComponent<RectTransform>().transform.position = new Vector3(doorUI.mainPutBackBG.transform.position.x, 70, 0);
                                    Interacting();
                                    inspected = hit.transform.gameObject;
                                    originalPos = hit.transform.position;
                                    originalRot = hit.transform.rotation;
                                    inspected.transform.SetParent(examineItemPos);
                                }
                            }
                            else if (Input.GetKeyDown(doorUI.interactKey) && examining)
                            {
                                if (blue || black || green || red)
                                {
                                    lockController.KeycardAccept();
                                }
                                else if (doorUI.blueKeycard && !doorUI.blueKeycardDoorOpened && lockController.keycard == LockController.keycards.BlueCardReader)
                                {
                                    StartCoroutine(lockController.KeycardSound());
                                    hit.collider.gameObject.GetComponent<Animation>().clip = lockController.blueKeycardAnim;
                                    hit.collider.gameObject.GetComponent<Animation>().Play();
                                    StartCoroutine(LockOpen());
                                    doorUI.blueKeycardDoorOpened = true;
                                }
                                else if (doorUI.redKeyCard && !doorUI.redKeycardDoorOpened && lockController.keycard == LockController.keycards.RedCardReader)
                                {
                                    StartCoroutine(lockController.KeycardSound());
                                    hit.collider.gameObject.GetComponent<Animation>().clip = lockController.blueKeycardAnim;
                                    hit.collider.gameObject.GetComponent<Animation>().Play();
                                    StartCoroutine(LockOpen());
                                    doorUI.redKeycardDoorOpened = true;
                                }
                                else if (doorUI.greenKeyCard && !doorUI.greenKeycardDoorOpened && lockController.keycard == LockController.keycards.GreenCardReader)
                                {
                                    StartCoroutine(lockController.KeycardSound());
                                    hit.collider.gameObject.GetComponent<Animation>().clip = lockController.blueKeycardAnim;
                                    hit.collider.gameObject.GetComponent<Animation>().Play();
                                    StartCoroutine(LockOpen());
                                    doorUI.greenKeycardDoorOpened = true;
                                }
                                else if (doorUI.blackKeyCard && !doorUI.blackKeycardDoorOpened && lockController.keycard == LockController.keycards.BlackCardReader)
                                {
                                    StartCoroutine(lockController.KeycardSound());
                                    hit.collider.gameObject.GetComponent<Animation>().clip = lockController.blueKeycardAnim;
                                    hit.collider.gameObject.GetComponent<Animation>().Play();
                                    StartCoroutine(LockOpen());
                                    doorUI.blackKeycardDoorOpened = true;
                                }
                                else
                                {
                                    lockController.audioSource.clip = lockController.wrongCodeSound;
                                    lockController.audioSource.Play();
                                    lockController.KeycardDecline();
                                }
                            }
                            OnExamining();
                        }

                        #endregion
                    }        
                    #endregion
                }
                //Lock Opener
                else if (lockOpener = hit.collider.GetComponent<LockOpener>())
                {
                    #region Locks Opener
                    #region NormalLock Keys
                    if (lockOpener.doorKeys.blueKey)
                    {
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { Interaction(); }
                        if (interact)
                        {
                            doorUI.blueKey = true; 
                            Destroy(hit.collider.gameObject);
                        }
                    }
                    else if (lockOpener.doorKeys.redKey)  
                    {
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { Interaction(); }
                        if (interact)
                        {
                            doorUI.redKey = true;
                            Destroy(hit.collider.gameObject);
                        }
                    }
                    else if (lockOpener.doorKeys.greenKey)
                    {
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { Interaction(); }
                        if (interact)
                        {
                            doorUI.greenKey = true;
                            Destroy(hit.collider.gameObject);
                        }
                    }
                    else if (lockOpener.doorKeys.blackKey)
                    {
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { Interaction(); }
                        if (interact)
                        {
                            doorUI.blackKey = true;
                            Destroy(hit.collider.gameObject);
                        }
                    }
                    #endregion
                    #region Keycards
                    else if (lockOpener.keycards.blueKeycard)
                    {
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { Interaction(); }
                        if (interact)
                        {
                            doorUI.blueKeycard = true;
                            Destroy(hit.collider.gameObject);
                        }
                    }
                    else if (lockOpener.keycards.redKeycard)
                    {
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { Interaction(); }
                        if (interact)
                        {
                            doorUI.redKeyCard = true;
                            Destroy(hit.collider.gameObject);
                        }
                    }
                    else if (lockOpener.keycards.greenKeycard)
                    {
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { Interaction(); }
                        if (interact)
                        {
                            doorUI.greenKeyCard = true;
                            Destroy(hit.collider.gameObject);
                        }
                    }
                    else if (lockOpener.keycards.blackKeycard)
                    {
                        crosshairChangeColor = true;
                        if (crosshairChangeColor) { Interaction(); }
                        if (interact)
                        {
                            doorUI.blackKeyCard = true;
                            Destroy(hit.collider.gameObject);
                        }
                    }
                    #endregion 
                    #endregion
                } 
            }
            else
            {
                crosshairChangeColor = false;
                if (!crosshairChangeColor)
                {
                    doorUI.crosshair.color = Color.white;
                    doorUI.InteractKeyBG.SetActive(false);
                    doorUI.holdDoorKeyBG.SetActive(false);
                    doorUI.doorKeyBG.SetActive(false);
                    doorUI.examineKeyBG.SetActive(false);
                }
            }

            if (time)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    timer = 0;
                    HideInfo();
                    time = false;
                }
            }
        }
        private void Interaction()
        {
            doorUI.crosshair.color = doorUI.unlockCrosshairColor;
            doorUI.InteractText.text = lockOpener.interactUI;
            doorUI.InteractKeyBG.SetActive(true);
        }
        private void LockedExamine()
        {
            lockController.ExamineLockedText();
            time = true;
            doorUI.doorLockedBG.SetActive(true);
            LeanTween.scale(doorUI.doorLockedBG, new Vector3(0, 0, 0), 0).setEase(LeanTweenType.linear);
            LeanTween.scale(doorUI.doorLockedBG, new Vector3(1, 1, 1), doorUI.textAnimationSpeed / 10).setEase(LeanTweenType.linear);
            timer = 2;
        }
        private void NormalLock()
        {
            StartCoroutine(WaitTime());
            antiAnimationRepeat = true; 
            StartCoroutine(Lock());
        }
        IEnumerator WaitTime()
        {
            yield return new WaitForSeconds(0.2f);
            lockController.audioSource.clip = lockController.useKeySound;
            lockController.audioSource.Play();
        }
        private void Interacting()
        {
            examining = true;
            doorUI.mainPutBackBG.SetActive(true);
            ExamineTextAnimations();
        }
        private void ExamineAndInteract()
        {
            doorUI.crosshair.color = doorUI.unlockCrosshairColor; doorUI.examineKeyBG.SetActive(true);
            doorUI.InteractKeyBG.SetActive(false);
        }
        private void DoorLockSound()
        {
            time = true;
            doorUI.doorLockedBG.SetActive(true); LeanTween.scale(doorUI.doorLockedBG, new Vector3(0, 0, 0), 0).setEase(LeanTweenType.linear);
            TextAnimation();
            doorController.audioSource.clip = doorController.doorLockSound;
            doorController.audioSource.Play();
        }
        private void DoorAndExamine()
        {
            doorUI.crosshair.color = doorUI.unlockCrosshairColor; doorUI.doorKeyBG.SetActive(true);
            doorUI.examineKeyBG.SetActive(false);
        }
        private void InteractAndExamine()
        {
            doorUI.crosshair.color = doorUI.lockCrosshairColor; doorUI.InteractText.text = doorController.lockedDoorUI; doorUI.InteractKeyBG.SetActive(true);
            doorUI.examineKeyBG.SetActive(false);
        }
        private void DoorInteraction()
        { 
            if (doorController.isOpen)
            {
                doorUI.doorsSateText.text = doorController.closeDoorUI;
            }
            else
            {
                doorUI.doorsSateText.text = doorController.openDoorUI;
            }
        }
        private void ExamineTextAnimations()
        {
            LeanTween.scale(doorUI.doorLockedBG, new Vector3(0, 0, 0), 0).setEase(LeanTweenType.linear);
            LeanTween.scale(doorUI.mainPutBackBG, new Vector3(1, 1, 1), doorUI.textAnimationSpeed / 10).setEase(LeanTweenType.linear);
        }
        private void TextAnimation()
        {
            doorController.DoorLockedText();
            LeanTween.scale(doorUI.doorLockedBG, new Vector3(1.05f, 1.05f, 1.05f), doorUI.textAnimationSpeed / 10).setEase(LeanTweenType.linear);
            timer = doorController.textDisplayTimer;
        }
        void OnExamining()
        {
            if (examining)
            {
                doorUI.examineKeyBG.SetActive(false);
                doorUI.crosshair.enabled = false;
                if(lockController.lockType == LockController.locks.Padlock && padLockDoor) {
                    padlockPos.gameObject.SetActive(true);
                    inspected.transform.position = Vector3.Lerp(inspected.transform.position, padlockPos.position, 10 * Time.deltaTime); }
                else { inspected.transform.position = Vector3.Lerp(inspected.transform.position, examineItemPos.position, 10 * Time.deltaTime); }
                playerMovement.enabled = false;
                if (lockController && examineCam != null) { inspected.transform.rotation = Quaternion.RotateTowards(inspected.transform.rotation, Quaternion.LookRotation(examineCam.transform.forward, examineCam.transform.up) * Quaternion.Euler(lockController.itemRotation), (10 * 100) * Time.deltaTime); }
                SetFocused(inspected);
                myVolume.profile = examineBlur;
                StartCoroutine(DropItem());
                doorUI.startAgain = true;
            }
            else if (inspected != null)
            {
                Back();
           }
        }
        void SetFocused(GameObject obj)
        {
            examineCam.SetActive(true);
            if (currentlyFocused) currentlyFocused.layer = previousLayer;
            currentlyFocused = obj;
            if (currentlyFocused)
            {
                previousLayer = currentlyFocused.layer;
                foreach (Transform child in currentlyFocused.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = LayerMask.NameToLayer(examineLayerName);
                }
            }
            else
            {
                foreach (Transform child in inspected.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = previousLayer;
                }
            }
        }
        private void HideInfo()
        {
            LeanTween.scale(doorUI.doorLockedBG, new Vector3(0, 0, 0), doorUI.textAnimationSpeed / 10).setEase(LeanTweenType.linear);
        }
        private void HideExamineInfo()
        {
            LeanTween.scale(doorUI.doorLockedBG, new Vector3(0, 0, 0), doorUI.textAnimationSpeed / 10).setEase(LeanTweenType.linear);
            LeanTween.scale(doorUI.mainPutBackBG, new Vector3(0, 0, 0), doorUI.textAnimationSpeed / 10).setEase(LeanTweenType.linear);
        }
        void Back()
        {
            if (inspected != null)
            {
                inspected.transform.SetParent(null);
                inspected.transform.position = Vector3.Lerp(inspected.transform.position, originalPos, 10 * Time.deltaTime);
                inspected.transform.rotation = Quaternion.Lerp(inspected.transform.rotation, originalRot,10 * Time.deltaTime);
            }
        }
        IEnumerator DoorUnlock()
        {
            doorController.audioSource.clip = doorController.useKeySound;
            doorController.audioSource.Play();
            playerMovement.enabled = false;
            yield return new WaitForSeconds(2);
            playerMovement.enabled = true;
            if(doorUI.blueKey && !doorUI.bKey)
            {
                doorUI.bKey = true;
            }
            else if(doorUI.redKey && !doorUI.rKey) { doorUI.rKey = true; }
            else if (doorUI.greenKey && !doorUI.gKey) { doorUI.gKey = true; }
            else if (doorUI.blackKey && !doorUI.bbKey) { doorUI.bbKey = true; }
        }
        IEnumerator DoorUnlockk()
        {
            doorController.audioSource.clip = doorController.useKeySound;
            doorController.audioSource.Play();
            playerMovement.enabled = false;
            doorController.GetComponent<Animation>().clip = doorController.keyAnim;
            doorController.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(1.75f);
            doorController.keyDoorAnim = false;
            playerMovement.enabled = true;
            if (doorUI.blueKey && !doorUI.bkKey) { doorUI.bkKey = true; }
            else if (doorUI.redKey && !doorUI.rrKey) { doorUI.rrKey = true; }
            else if (doorUI.greenKey && !doorUI.ggKey) { doorUI.ggKey = true; }
            else if (doorUI.blackKey && !doorUI.blKey) { doorUI.blKey = true; }
        }
        IEnumerator DropItem()
        {
            yield return new WaitForSeconds(lockController.antiSpamTime); 
            if (Input.GetKeyDown(doorUI.examineKey) && examining && !antiAnimationRepeat)  
            {
                if(lockController.lockType == LockController.locks.Padlock)
                {
                    padlockPos.gameObject.SetActive(false);
                    StartCoroutine(lockController.PadLockOpen());
                }
                antiAnimationRepeat = false;
                HideExamineInfo();
                myVolume.profile = defaultProfile;
                doorUI.crosshair.enabled = true;
                examining = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                yield return new WaitForSeconds(0.3f);
                playerMovement.enabled = true;
                yield return new WaitForSeconds(1);
                examineCam.SetActive(false);
                examineCam.GetComponent<Camera>().fieldOfView = examineOriginalFOV;
                _cam.fieldOfView = originalFOV;
                SetFocused(null);
                doorUI.startAgain = false;
            }
        }
        public IEnumerator Lock()
        {  
            if(lockController.lockType == LockController.locks.NormalLock)
            {
                yield return new WaitForSeconds(lockController.keyLockAnim.length + 0.2f);
                LeanTween.scale(doorUI.mainPutBackBG, new Vector3(0, 0, 0), doorUI.textAnimationSpeed / 10).setEase(LeanTweenType.linear);
            }
            HideExamineInfo();
            myVolume.profile = defaultProfile;
            doorUI.crosshair.enabled = true;
            examining = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;           
            yield return new WaitForSeconds(1);
            examineCam.SetActive(false);
            examineCam.GetComponent<Camera>().fieldOfView = examineOriginalFOV;
            _cam.fieldOfView = originalFOV;
            SetFocused(null);
            yield return new WaitForSeconds(0.3f);
            if (lockController.lockType == LockController.locks.NormalLock)
            {
                inspected.GetComponent<Animation>().clip = lockController.lockOpenAnim;
                inspected.GetComponent<Animation>().Play();
            }
            yield return new WaitForSeconds(1);
            playerMovement.enabled = true;
            Destroy(inspected);
            yield return new WaitForSeconds(1.3f); 
            doorUI.startAgain = false;
            antiAnimationRepeat = false;
        }
        public IEnumerator LockOpen()
        {
            if (lockController.lockType == LockController.locks.Padlock)
            {
                padlockPos.gameObject.SetActive(false);
                StartCoroutine(lockController.PadLockOpen());
            }
            if (lockController.lockType == LockController.locks.CardReader)
            {
                antiAnimationRepeat = true;
                yield return new WaitForSeconds(lockController.blueKeycardAnim.length + 0.5f);
                lockController.audioSource.clip = lockController.rightCodeSound;
                lockController.audioSource.Play();
                lockController.KeycardAccept();
                antiAnimationRepeat = false;
                yield break;
            }
            HideExamineInfo();
            myVolume.profile = defaultProfile;
            doorUI.crosshair.enabled = true;
            examining = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            SetFocused(null);
            yield return new WaitForSeconds(0.3f);
            if (lockController.lockType == LockController.locks.CardReader) { Destroy(null); }
            else if (lockController.lockType == LockController.locks.Padlock)
            {
                yield return new WaitForSeconds(lockController.padLockOpen.length + 0.5f);
                Destroy(inspected); 
                playerMovement.enabled = true;
                yield return new WaitForSeconds(1);
                examineCam.SetActive(false);
                examineCam.GetComponent<Camera>().fieldOfView = examineOriginalFOV;
                _cam.fieldOfView = originalFOV;
                yield return new WaitForSeconds(lockController.antiSpamTime);
                doorUI.startAgain = false;
                antiAnimationRepeat = false;
                yield break;
            }
            yield return new WaitForSeconds(1.5f);
            playerMovement.enabled = true;
            yield return new WaitForSeconds(1);
            examineCam.SetActive(false);
            examineCam.GetComponent<Camera>().fieldOfView = examineOriginalFOV;
            _cam.fieldOfView = originalFOV;
            yield return new WaitForSeconds(lockController.antiSpamTime); 
            doorUI.startAgain = false;
            antiAnimationRepeat = false;
        }
    }

   
}
