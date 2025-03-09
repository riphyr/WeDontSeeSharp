using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class PadLock : MonoBehaviourPun
    {
        [Header("Paramètres du cadenas à code")]
        public string correctCode = "1234";

        [Header("Références des roues (Automatique)")]
        private Transform WheelOne;
        private Transform WheelTwo;
        private Transform WheelThree;
        private Transform WheelFour;

        [Header("Références caméras et joueur (Automatique)")]
        private Camera playerCamera;
        public Camera padLockCamera;
        private PlayerScript playerScript;
        private CameraLookingAt cameraLookingAt;
        private GameObject gui;
        private PhotonView view;

        [Header("Audio")]
        public AudioSource asource;
        public AudioClip rotateClick;
        public AudioClip unlockSuccess;

        private string currentCombination = "0000";

        void Start()
        {
            asource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();

            WheelFour = transform.Find("WheelOne");
            WheelThree = transform.Find("WheelTwo");
            WheelTwo = transform.Find("WheelThree");
            WheelOne = transform.Find("WheelFour");
            
            StartCoroutine(WaitForPlayer());
        }

        private IEnumerator WaitForPlayer()
        {
            GameObject localPlayer = null;

            while (localPlayer == null)
            {
                localPlayer = PhotonNetwork.LocalPlayer.TagObject as GameObject;
                yield return null;
            }

            Transform playerTransform = localPlayer.transform.Find("Player");

            if (playerTransform != null)
            {
                playerCamera = playerTransform.Find("Main Camera")?.GetComponent<Camera>();
                playerScript = playerTransform.GetComponent<PlayerScript>();
                cameraLookingAt = playerTransform.GetComponent<CameraLookingAt>();
            }
            
            gui = localPlayer.transform.Find("GUI")?.gameObject;
        }

        
        void Update()
        {
            if (padLockCamera.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ExitPadLockMode();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    DetectWheelClick();
                }
            }
        }

        public void EnterPadLockMode()
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            playerCamera.gameObject.SetActive(false);
            padLockCamera.gameObject.SetActive(true);

            playerScript.enabled = false;
            cameraLookingAt.enabled = false;
            gui.SetActive(false);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ExitPadLockMode()
        {
            padLockCamera.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            playerScript.enabled = true;
            cameraLookingAt.enabled = true;
            gui.SetActive(true);
        }
        
        private void DetectWheelClick()
        {
            Ray ray = padLockCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("PadLock")))
            {
                if (hit.transform == WheelOne) 
                {
                    RotateWheel(0);
                }
                else if (hit.transform == WheelTwo) 
                {
                    RotateWheel(1);
                }
                else if (hit.transform == WheelThree) 
                {
                    RotateWheel(2);
                }
                else if (hit.transform == WheelFour) 
                {
                    RotateWheel(3);
                }
            }
        }


        public void RotateWheel(int wheelIndex)
        {
            if (!padLockCamera.gameObject.activeSelf) 
                return;

            switch (wheelIndex)
            {
                case 0:
                {
                    RotateWheelLogic(WheelOne, 0);
                    break;
                }
                case 1:
                {
                    RotateWheelLogic(WheelTwo, 1);
                    break;
                }
                case 2:
                {
                    RotateWheelLogic(WheelThree, 2);
                    break;
                }
                case 3:
                {
                    RotateWheelLogic(WheelFour, 3);
                    break;
                }
            }

            if (currentCombination == correctCode)
            {
                if (!view.IsMine)
                {
                    view.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
                
                StartCoroutine(UnlockPadLock());
            }
        }

        private void RotateWheelLogic(Transform wheel, int wheelPosition)
        {
            if (currentCombination != correctCode)
            {
                wheel.Rotate(0, 36f, 0);

                int currentDigit = (currentCombination[wheelPosition] - '0' + 1) % 10;
                char[] newCombination = currentCombination.ToCharArray();
                newCombination[wheelPosition] = (char)('0' + currentDigit);
                currentCombination = new string(newCombination);

                asource.PlayOneShot(rotateClick, 1.0f);
            }
        }

        private IEnumerator UnlockPadLock()
        {
            asource.PlayOneShot(unlockSuccess, 1.0f);
            yield return new WaitForSeconds(unlockSuccess.length);
            
            if (view.IsMine)
            {
                ExitPadLockMode();
                view.RPC("DisablePadLockForAll", RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        private void DisablePadLockForAll()
        {
            gameObject.SetActive(false);
        }
    }
}
