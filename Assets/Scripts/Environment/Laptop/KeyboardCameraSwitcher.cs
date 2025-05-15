using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class KeyboardCameraSwitcher : MonoBehaviourPun
    {
        public Camera[] securityCameras;
        public RenderTexture[] renderTextures;
        public Material screenMaterial;
        private AudioSource audioSource;
        public AudioClip mouseSound;
        public AudioClip startupSound;

        private int currentCameraIndex = 0;
        private bool isPoweredOn = false;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            
            foreach (Camera cam in securityCameras)
            {
                cam.enabled = false;
            }
        }
        
        public bool isOn()
        {
            return isPoweredOn;
        }

        public void PowerOnSystem()
        {
            if (isPoweredOn)
                return;

            isPoweredOn = true;

            photonView.RPC("RPC_PlayStartupSound", RpcTarget.All);

            FindObjectOfType<ScreenInteraction>()?.PowerOnScreen();
        }
        
        [PunRPC]
        private void RPC_PlayStartupSound()
        {
            StartCoroutine(PlaySoundAndTurnOn());
        }
        
        private IEnumerator PlaySoundAndTurnOn()
        {
            yield return new WaitForSeconds(2f);
            audioSource.PlayOneShot(startupSound);
            photonView.RPC("RPC_PowerOnSystem", RpcTarget.AllBuffered);
        }

        [PunRPC]
        public void RPC_PowerOnSystem()
        {
            isPoweredOn = true;
        }

        public void NextCamera()
        {
            if (!isPoweredOn)
            {
                return;
            }

            if (!photonView.IsMine)
            {
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            int newIndex = (currentCameraIndex + 1) % securityCameras.Length;
            photonView.RPC("RPC_SwitchCamera", RpcTarget.AllBuffered, newIndex);
        }

        [PunRPC]
        public void RPC_SwitchCamera(int index)
        {
            if (index < 0 || index >= securityCameras.Length)
                return;

            audioSource.PlayOneShot(mouseSound);

            foreach (Camera cam in securityCameras)
            {
                cam.enabled = false;
            }

            securityCameras[index].enabled = true;
            screenMaterial.mainTexture = renderTextures[index];
            currentCameraIndex = index;
        }
    }
}
