
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;
using UnityEngine.UI;

namespace EnivStudios
{
    public class DoorUI : MonoBehaviour
    {
        [Header("Crosshair UI")]
        public Image crosshair;
        public Color lockCrosshairColor;
        public Color unlockCrosshairColor;

        [Header("Interact")]
        public KeyCode interactKey;
        public GameObject InteractKeyBG;
        public Text InteractText;
        public Text InteractKeyCode;

        [Header("Examine")]
        public KeyCode examineKey;
        public GameObject examineKeyBG;
        public Text examineKeyCode;

        [Header("Door Interact")]
        public GameObject doorKeyBG;
        public Text doorKeyCode;
        public Text doorsSateText;

        [Header("Hold Door Interact")]
        public GameObject holdDoorKeyBG;
        public Text holdDoorKeyCode;
        public Text holdDoorSateText;
        public Image backgroundCircle;

        [Header("Locked Door")]
        public GameObject doorLockedBG;
        public Text doorLockedText;

        [Header("Other UI")]
        public GameObject mainPutBackBG;
        public GameObject circleBG;
        public GameObject mouseBG;

        [Space()]
        public float textAnimationSpeed;

        [HideInInspector] public bool blueKey, redKey, blueDoorOpened, redDoorOpened, keypadDoorOpned, greenKey, greenDoorOpened, blackKey, blackDoorOpened;
        [HideInInspector] public bool doorState, wrongCode, padlockDoorOpened,blueKeycard, blueKeycardDoorOpened;
        [HideInInspector] public bool redKeycardDoorOpened, greenKeycardDoorOpened, blackKeycardDoorOpened, redKeyCard, greenKeyCard, blackKeyCard;
        [HideInInspector] public bool bKey, rKey, gKey, bbKey,bkKey,rrKey,ggKey,blKey,bOpen,rOpen,gOpen,bbOpen,bdOpen,rdOpen,gdOpen,bldOpen, startAgain;
    }
}
