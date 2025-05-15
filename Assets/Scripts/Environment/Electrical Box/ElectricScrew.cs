using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class ElectricScrew : MonoBehaviourPun
    {
        private bool isRemoving = false;
        public float removalTime = 2f;
        public AudioClip removeSound;
        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void TryRemoveScrew()
        {
            if (isRemoving) 
                return;

            isRemoving = true;
            photonView.RPC("RPC_StartRemoving", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_StartRemoving()
        {
            StartCoroutine(RemoveScrew());
        }

        private IEnumerator RemoveScrew()
        {
            audioSource.PlayOneShot(removeSound, 0.4f);
            float timer = 0f;

            Vector3 startPos = transform.localPosition;
            Vector3 endPos = startPos + new Vector3(0, 0, 0.02f);

            Quaternion startRotation = transform.localRotation;

            while (timer < removalTime)
            {
                timer += Time.deltaTime;
        
                float rotationX = startRotation.eulerAngles.x - (360 * Time.deltaTime / removalTime);
                transform.localRotation = Quaternion.Euler(rotationX, startRotation.eulerAngles.y, startRotation.eulerAngles.z);

                transform.localPosition = new Vector3(startPos.x, startPos.y, Mathf.Lerp(startPos.z, endPos.z, timer / removalTime));

                yield return null;
            }

            photonView.RPC("RPC_DestroyScrew", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RPC_DestroyScrew()
        {
            Destroy(gameObject);
        }
    }
}