using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class ElectricButton : MonoBehaviourPun
    {
        private bool isOn = false;
        private Vector3 onPosition = new Vector3(-0.1f, 0, 0);
        private Vector3 offPosition = new Vector3(0.035f, 0, 0);
        public float switchSpeed = 10f;
        private float initialY;
        private float initialZ;

        private PhotonView photonView;
        private AudioSource audioSource;
        public AudioClip toggleSound;

        private void Start()
        {
            photonView = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            
            initialY = transform.localPosition.y;
            initialZ = transform.localPosition.z;
        }

        public bool IsOn()
        {
            return isOn;
        }

        public void ToggleButton()
        {
            if (!photonView.IsMine) 
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            
            isOn = !isOn;

            photonView.RPC("RPC_ToggleButton", RpcTarget.All, isOn);
        }

        [PunRPC]
        private void RPC_ToggleButton(bool newState)
        {
            isOn = newState;

            audioSource.PlayOneShot(toggleSound);
            
            StopAllCoroutines();
            StartCoroutine(AnimateButton(isOn ? onPosition : offPosition));
        }

        private IEnumerator AnimateButton(Vector3 targetPosition)
        {
            Vector3 startPosition = transform.localPosition;
            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime * switchSpeed;

                Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, t);
                transform.localPosition = new Vector3(newPosition.x, initialY, initialZ);

                yield return null;
            }
        }
    }
}
