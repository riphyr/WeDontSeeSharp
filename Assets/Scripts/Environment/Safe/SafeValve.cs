using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    public class SafeValve : MonoBehaviourPun
    {
        public Transform valveTransform;
        public SafeDial safeDial;
        public SafeDoor safeDoor;
        public AudioClip unlockSound;
        public AudioClip failSound;
        public BoxCollider collider;

        private AudioSource audioSource;
        private bool isUnlocked = false;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        [PunRPC]
        public void RPC_SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
        }
        
        public void TryUnlock()
        {
            if (!photonView.IsMine)
                photonView.RequestOwnership();

            photonView.RPC("RPC_TryUnlock", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RPC_TryUnlock()
        {
            safeDial.RegisterCurrentNumber();

            if (isUnlocked)
                StartCoroutine(RotateValve());
            else
                StartCoroutine(FailedUnlock());

            ResetSafeDial();
        }


        private void ResetSafeDial()
        {
            if (safeDial != null)
            {
                safeDial.ResetCombination();
            }
        }
        
        private IEnumerator RotateValve()
        {
            audioSource.PlayOneShot(unlockSound);
            float duration = 1.5f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                valveTransform.Rotate(Vector3.forward, 5f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            collider.enabled = false;
            safeDoor.OpenDoor();
        }

        private IEnumerator FailedUnlock()
        {
            audioSource.PlayOneShot(failSound);
            float startRotation = valveTransform.localEulerAngles.z;
            float targetRotation = startRotation + 10f;

            float elapsed = 0;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                valveTransform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(startRotation, targetRotation, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            valveTransform.localEulerAngles = new Vector3(0, 0, startRotation);
        }
    }
}
