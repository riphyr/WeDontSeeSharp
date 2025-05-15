using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class SafeDoor : MonoBehaviourPun
    {
        public Transform openPosition;
        public AudioClip doorOpenSound;
        
        private bool isOpen = false;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private AudioSource audioSource;

        private void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            audioSource = GetComponent<AudioSource>();
        }

        public void OpenDoor()
        {
            if (isOpen) return;
            isOpen = true;
            photonView.RPC("RPC_OpenDoor", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RPC_OpenDoor()
        {
            StartCoroutine(MoveDoor());
        }

        private IEnumerator MoveDoor()
        {
            float duration = 1.5f;
            float elapsed = 0;
            
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            if (doorOpenSound != null)
                audioSource.PlayOneShot(doorOpenSound);

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, openPosition.position, elapsed / duration);
                transform.rotation = Quaternion.Lerp(startRot, openPosition.rotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = openPosition.position;
            transform.rotation = openPosition.rotation;
        }
    }
}