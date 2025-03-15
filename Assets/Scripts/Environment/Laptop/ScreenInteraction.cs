using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    public class ScreenInteraction : MonoBehaviourPun
    {
        public Camera screenCamera;
        public Transform keyboard;
        private KeyboardCameraSwitcher keyboardCameraSwitcher;
        private Camera playerCamera;
        private PlayerScript playerScript;
        private CameraLookingAt cameraLookingAt;
        private PlayerUsing playerUsing;
        private GameObject gui;

        private bool isViewingScreen = false;
        private bool isPCOn = false;

        void Start()
        {
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
                playerUsing = playerTransform.GetComponent<PlayerUsing>();
            }
            
            gui = localPlayer.transform.Find("GUI")?.gameObject;
            
            keyboardCameraSwitcher = keyboard.GetComponent<KeyboardCameraSwitcher>();
        }

        void Update()
        {
            if (isViewingScreen)
            {
                if (Input.GetKeyDown(GetKeyCodeFromString(PlayerPrefs.GetString("Pause"))))
                {
                    ExitScreenMode();
                }

                if (Input.GetKeyDown(GetKeyCodeFromString(PlayerPrefs.GetString("PrimaryInteraction"))))
                {
                    keyboardCameraSwitcher.NextCamera();
                }
            }
        }
        
        private KeyCode GetKeyCodeFromString(string key)
        {
            return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
        }

        public void EnterScreenMode()
        {
            if (!isPCOn)
            {
                return;
            }

            if (!photonView.IsMine)
            {
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            isViewingScreen = true;

            playerCamera.gameObject.SetActive(false);
            screenCamera.gameObject.SetActive(true);

            playerScript.enabled = false;
            cameraLookingAt.enabled = false;
            playerUsing.enabled = false;
            gui.SetActive(false);
        }

        public void ExitScreenMode()
        {
            isViewingScreen = false;

            screenCamera.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartCoroutine(ExitWithDelay());
        }
        
        private IEnumerator ExitWithDelay()
        {
            yield return new WaitForSeconds(0.1f);
            playerScript.enabled = true;
            cameraLookingAt.enabled = true;
            playerUsing.enabled = true;
            gui.SetActive(true);
        }

        public void PowerOnScreen()
        {
            photonView.RPC("RPC_PowerOnScreen", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RPC_PowerOnScreen()
        {
            isPCOn = true;
        }
        
        public bool isOn()
        {
            return isPCOn;
        }
    }
}
