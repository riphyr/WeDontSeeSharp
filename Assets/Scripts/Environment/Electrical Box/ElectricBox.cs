using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class ElectricBox : MonoBehaviourPun
    {
        private bool isOpen = false;
        public float openRotation = -90f;
        public float closeRotation = 70f;
        public float openSpeed = 2f;

        public AudioClip openSound;
        public AudioClip closeSound;
        public AudioClip lockedSound;
        private AudioSource audioSource;
        private PhotonView photonView;

        public ElectricScrew[] screws;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            photonView = GetComponent<PhotonView>();
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public void ToggleBox()
        {
            if (!photonView.IsMine)
            {
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            if (!AreScrewsRemoved()) 
            {
                Debug.Log("🔒 Impossible d'ouvrir, les vis sont encore là !");
                audioSource.PlayOneShot(lockedSound);
                return;
            }

            isOpen = !isOpen;
            photonView.RPC("RPC_ToggleBox", RpcTarget.All, isOpen);
        }

        [PunRPC]
        private void RPC_ToggleBox(bool openState)
        {
            isOpen = openState;

            if (isOpen)
            {
                audioSource.PlayOneShot(openSound);
                StopAllCoroutines();
                StartCoroutine(AnimateCover(closeRotation));
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(AnimateCover(openRotation));
                StartCoroutine(DelayedCloseSound());
            }
        }

        private IEnumerator DelayedCloseSound()
        {
            yield return new WaitForSeconds(0.25f);
            audioSource.PlayOneShot(closeSound);
        }

        private IEnumerator AnimateCover(float targetRotationX)
        {
            Quaternion startRotation = transform.localRotation;
            Quaternion endRotation = Quaternion.Euler(targetRotationX, 90, 0);
            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime * openSpeed;

                Quaternion currentRotation = Quaternion.Slerp(startRotation, endRotation, t);
                transform.localRotation = Quaternion.Euler(currentRotation.eulerAngles.x, 90, 0);

                yield return null;
            }
        }

        private bool AreScrewsRemoved()
        {
            foreach (ElectricScrew screw in screws)
            {
                if (screw != null) 
                    return false;
            }
            return true;
        }
    }
}
