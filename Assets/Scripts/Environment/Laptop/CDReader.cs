using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class CDReader : MonoBehaviourPun
    {
        public Transform cdSlotPosition;
        public Transform closedPosition;
        public AudioClip readSound;
        [HideInInspector]public GameObject cdObject;
        public Transform trayTransform;

        private AudioSource audioSource;
        private bool isOpen = true;
        private bool isReading = false;
        private bool hasCD = false;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void InsertCD(GameObject cd)
        {
            if (!isOpen || hasCD)
                return;

            hasCD = true;
            cdObject = cd;

            photonView.RPC("RPC_InsertCD", RpcTarget.AllBuffered, cd.GetComponent<PhotonView>().ViewID);
        }

        [PunRPC]
        private void RPC_InsertCD(int cdViewID)
        {
            PhotonView cdView = PhotonView.Find(cdViewID);
            if (cdView != null)
            {
                cdObject = cdView.gameObject;
                StartCoroutine(AnimateCDInsertion(cdView.gameObject));
            }
        }
        
        private IEnumerator AnimateCDInsertion(GameObject cd)
        {
            float duration = 1.5f;
            float elapsedTime = 0f;
            Vector3 startPosition = cd.transform.position;
            Quaternion startRotation = cd.transform.rotation;

            Vector3 endPosition = cdSlotPosition.position;
            Quaternion endRotation = cdSlotPosition.rotation;

            while (elapsedTime < duration)
            {
                cd.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
                cd.transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            cd.transform.position = endPosition;
            cd.transform.rotation = endRotation;

            yield return new WaitForSeconds(0.5f);

            CloseCDReader();
            
            cd.GetComponent<CDDisk>().enabled = false;
        }

        private void CloseCDReader()
        {
            photonView.RPC("RPC_CloseCDReader", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RPC_CloseCDReader()
        {
            if (!isOpen) return;

            isOpen = false;
            StartCoroutine(AnimateClosing());
        }

        private IEnumerator AnimateClosing()
        {
            float duration = 1.5f;
            float elapsedTime = 0f;

            Vector3 startTrayPosition = trayTransform.position;
            Vector3 endTrayPosition = closedPosition.position;

            Vector3 startCDPosition = cdObject.transform.position;
            Vector3 endCDPosition = closedPosition.position;
            
            while (elapsedTime < duration)
            {
                trayTransform.position = Vector3.Lerp(startTrayPosition, endTrayPosition, elapsedTime / duration);
                cdObject.transform.position = Vector3.Lerp(startCDPosition, endCDPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            trayTransform.position = endTrayPosition;
            cdObject.transform.position = endCDPosition;

            StartCoroutine(StartCDReading());
        }
        
        private IEnumerator StartCDReading()
        {
            yield return new WaitForSeconds(1f);
            isReading = true;
            audioSource.PlayOneShot(readSound);

            KeyboardCameraSwitcher cameraSwitcher = FindObjectOfType<KeyboardCameraSwitcher>();
            if (cameraSwitcher != null)
            {
                cameraSwitcher.PowerOnSystem();
            }

            yield return new WaitForSeconds(5f);
            if (cdObject != null && cdObject.GetComponent<PhotonView>().IsMine)
            {
                PhotonNetwork.Destroy(cdObject);
            }
        }
        
        public bool CanInsertCD()
        {
            return isOpen && !hasCD;
        }
    }
}
