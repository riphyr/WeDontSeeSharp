
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace EnivStudios
{
    public class LockController : MonoBehaviour
    {
        public enum locks { NormalLock, Keypad, Padlock, CardReader }
        public locks lockType;

        public enum normallock { BlueKeyLock, RedKeyLock, GreenKeyLock, BlackKeyLock }
        public normallock normallockType;
        public enum anims { NoAnimation, KeycardAnimation }
        public anims keycardAnimationType;
        public enum keycards { BlueCardReader, RedCardReader, GreenCardReader, BlackCardReader }
        public keycards keycard;

        [SerializeField] AudioClip leverSound;
        public string keypadNumber;
        [SerializeField] private int combination;
        public Vector3 itemRotation;
        [HideInInspector] public bool padLockGear,padLockStop,NoAnimation,KeycardAnimation;
        public AnimationClip blueKeycardAnim, keyLockAnim, lockOpenAnim, padLockOpen;
        public string examineLockedText;
        public Text keycardText;
        [SerializeField] RaycastDoor raycastDoor;
        [SerializeField] DoorUI doorUI;
        private int[] correctCombination, result;
        private Animation anim;
        private bool changePos;
        bool time,yes,isLeverSound;
        [HideInInspector] public AudioSource audioSource;
        public AudioClip useKeySound, wrongCodeSound,rightCodeSound,rotateSound,insertSound;
        public float antiSpamTime = 1;
        [SerializeField] Text keypad;
        public string keypadNo;
        float timer,originalVolume;
   
        private void Start()
        {
            doorUI = FindObjectOfType<DoorUI>();
            raycastDoor = FindObjectOfType<RaycastDoor>();
            if (lockType == locks.NormalLock || lockType == locks.Keypad 
                || lockType == locks.Padlock || lockType == locks.CardReader)
            {
                audioSource = GetComponent<AudioSource>();
                originalVolume = audioSource.volume;
            }
            if (lockType == locks.Padlock)
            {
                anim = GetComponent<Animation>();
                result = combination.ToString().Select(o => int.Parse(o.ToString())).ToArray();
                correctCombination = new int[4];
            }
            if(keycardAnimationType == anims.NoAnimation) { NoAnimation = true; }
            if(keycardAnimationType == anims.KeycardAnimation) { KeycardAnimation = true;}
        }
        private void Update()
        {
            if (lockType == locks.Keypad)
            {
                if (keypad.text.Length > 10 && !doorUI.keypadDoorOpned)
                {
                    keypad.color = Color.red;
                    keypad.fontSize = 20;
                    keypad.text = "Maximum Length Reached";
                }
                else if (doorUI.keypadDoorOpned)
                {
                    keypad.color = Color.green;
                    keypad.fontSize = 30; keypad.text = "Door Unlocked";
                }
                else if (doorUI.wrongCode)
                {

                    keypad.color = Color.red;
                    keypad.fontSize = 30; keypad.text = "Wrong Code";
                    doorUI.wrongCode = false;
                }
            }
            if (time)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    timer = 0;
                    LeanTween.scale(doorUI.doorLockedBG, new Vector3(0, 0, 0), doorUI.textAnimationSpeed / 10).setEase(LeanTweenType.linear);
                    time = false;
                }
            }
        }
        public void ExamineLockedText()
        {
            doorUI.doorLockedText.text = examineLockedText;
        }

        #region Keypad
        public void KeypadNumber(int number)
        {
            if (keypad.text == "Wrong Code") { return; }
            keypad.color = Color.white;
            keypad.text += number.ToString();
        }
        public void ClearNumber()
        {
            keypad.fontSize = 40;
            if (keypad.text == "Door Unlocked" || keypad.text == "Wrong Code" ||
                keypad.text == "Maximum Length Reached") { keypad.text = keypad.text.Remove(0); }
            else if (keypad.text.Length > 0) { keypad.text = keypad.text.Remove(keypad.text.Length - 1); }
            else { return; }
        }
        public void Answer()
        {
            if (keypad.text == "Maximum Length Reached")
            {
                return;
            }
            if (keypad.text == keypadNo)
            {
                if (!yes) {
                    audioSource.volume = 0.6f;
                    audioSource.clip = rightCodeSound;
                    audioSource.Play();
                    yes = true;
                }
                doorUI.keypadDoorOpned = true;
            }
            if(keypad.text == "")
            {
                return;
            }
            else
            {
                if (!doorUI.keypadDoorOpned)
                {
                    audioSource.volume = 0.6f;
                    audioSource.clip = wrongCodeSound;
                    audioSource.Play();
                }
                doorUI.wrongCode = true;
            }
        }
        #endregion

        #region Padlock
        public void PadlockNo(int gearNo, int num)
        {
            if (gearNo == 1)
            {
                correctCombination[0] = num;
            }
            if (gearNo == 2)
            {
                correctCombination[1] = num;
            }
            if (gearNo == 3)
            {
                correctCombination[2] = num;
            }
            if (gearNo == 4)
            {
                correctCombination[3] = num;
            }
            if (result[0] == correctCombination[0] && result[1] == correctCombination[1] && result[2] == correctCombination[2] && result[3] == correctCombination[3])
            {
                padLockStop = true;
                raycastDoor.antiAnimationRepeat = true;
                doorUI.padlockDoorOpened = true;
                StartCoroutine(PadLock());
            }
        }
        public IEnumerator PadLockOpen()
        {
            if (padLockGear) { padLockGear = false; }
            yield return new WaitForSeconds(1f);
            padLockGear = true;
        }
        IEnumerator PadLock()
        {
            anim.clip = padLockOpen;
            anim.Play();
            yield return new WaitForSeconds(1);
            raycastDoor.StartCoroutine(raycastDoor.LockOpen());
        }
        #endregion

        #region Keycard
        public void KeycardAccept()
        {
            keycardText.color = Color.green;
            keycardText.text = "Access Granted";
        }
        public void KeycardDecline()
        {
            keycardText.color = Color.red;
            keycardText.text = "Access Denied";
        }
        public IEnumerator KeycardSound()
        {
            audioSource.clip = insertSound;
            audioSource.Play();
            yield return new WaitForSeconds(blueKeycardAnim.length / 2);
            audioSource.clip = insertSound;
            audioSource.Play();
        }
        #endregion
    }
}
