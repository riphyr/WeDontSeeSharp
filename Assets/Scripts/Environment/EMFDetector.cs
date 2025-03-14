using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class EMFDetector : MonoBehaviourPun
    {
        public Transform[] ledLights;
        public AudioClip pickupSound;
        public AudioClip beepSound;
        public float detectionRange = 15f;

        private AudioSource audioSource;
        private PhotonView view;
        private Transform ownerTransform;
        private bool isActive = false;
        private int currentLevel = 0;
        private Coroutine detectionCoroutine;

        void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            SetLEDLevel(0);
        }

        public void PickupEMF(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("EMFDetector");
            photonView.RPC("PlayPickupSound", RpcTarget.All);
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            StartCoroutine(PlaySoundAndDestroy());
        }

        private IEnumerator PlaySoundAndDestroy()
        {
            audioSource.PlayOneShot(pickupSound);
            yield return new WaitForSeconds(pickupSound.length);
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DestroyForAll()
        {
            Destroy(gameObject);
        }

        public void ToggleEMF()
        {
            isActive = !isActive;

            if (isActive)
            {
                if (detectionCoroutine == null)
                {
                    detectionCoroutine = StartCoroutine(EMFDetectionLoop());
                }
            }
            else
            {
                if (detectionCoroutine != null)
                {
                    StopCoroutine(detectionCoroutine);
                    detectionCoroutine = null;
                }
                SetLEDLevel(0);
                photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
            }
        }

        private IEnumerator EMFDetectionLoop()
        {
            while (isActive)
            {
                int newLevel = DetectEntities();

                if (newLevel != currentLevel)
                {
                    currentLevel = newLevel;
                    view.RPC("RPC_SetLEDLevel", RpcTarget.All, currentLevel);
                    view.RPC("RPC_PlayEMFBeep", RpcTarget.All);
                }

                yield return new WaitForSeconds(GetBeepFrequency(currentLevel));
            }
        }

        private int DetectEntities()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 15f);
            int detectedLevel = 0;

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Ghost"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    float heightDifference = Mathf.Abs(transform.position.y - collider.transform.position.y);

                    float obstructionFactor = GetObstructionFactor(collider.transform);

                    float heightFactor = Mathf.Clamp01(1.0f - (heightDifference / 3.0f));
                    float adjustedDistance = distance * (1.0f / heightFactor) * obstructionFactor; 

                    if (adjustedDistance < 3f) detectedLevel = 5;
                    else if (adjustedDistance < 5f) detectedLevel = Mathf.Max(detectedLevel, 4);
                    else if (adjustedDistance < 7f) detectedLevel = Mathf.Max(detectedLevel, 3);
                    else if (adjustedDistance < 10f) detectedLevel = Mathf.Max(detectedLevel, 2);
                    else if (adjustedDistance < 15f) detectedLevel = Mathf.Max(detectedLevel, 1);
                }
            }

            return detectedLevel;
        }
        
        private float GetObstructionFactor(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            float totalReduction = 1.0f;
            int validPaths = 0;
            int totalChecks = 15;

            for (int i = 0; i < totalChecks; i++)
            {
                Vector3 adjustedDirection = direction + new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-2f, 2f)
                );

                RaycastHit[] hits = Physics.RaycastAll(transform.position, adjustedDirection.normalized, detectionRange);
        
                float reduction = 1.0f;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform == target) 
                    {
                        validPaths++;
                        break;
                    }

                    if (hit.collider.CompareTag("Wall")) reduction *= 0.4f;
                    else if (hit.collider.CompareTag("Door")) reduction *= 0.7f;
                    else if (hit.collider.CompareTag("ThinObject")) reduction *= 0.9f;
                }

                totalReduction *= reduction;
            }

            if (validPaths > 0)
            {
                return Mathf.Clamp(totalReduction, 0.2f, 1.0f);
            }
    
            return 0.0f;
        }

        [PunRPC]
        private void RPC_SetLEDLevel(int level)
        {
            SetLEDLevel(level);
        }

        private void SetLEDLevel(int level)
        {
            for (int i = 0; i < ledLights.Length; i++)
            {
                ledLights[i].gameObject.SetActive(i < level);
            }
        }

        [PunRPC]
        private void RPC_PlayEMFBeep()
        {
            if (isActive && beepSound != null)
            {
                audioSource.PlayOneShot(beepSound);
            }
        }

        private float GetBeepFrequency(int level)
        {
            switch (level)
            {
                case 1: return 2.0f;
                case 2: return 1.5f;
                case 3: return 1.0f;
                case 4: return 0.6f;
                case 5: return 0.3f;
                default: return 2.0f;
            }
        }

        void Update()
        {
            if (ownerTransform != null)
            {
                transform.position = ownerTransform.position + ownerTransform.forward * 0.2f + ownerTransform.right * 0.1f + Vector3.up * 0.6f;
                transform.rotation = Quaternion.Euler(0f, ownerTransform.eulerAngles.y - 180f, 0f);
            }
        }

        public void AssignOwner(Photon.Realtime.Player player, Transform owner)
        {
            this.ownerTransform = owner;
            photonView.TransferOwnership(player);
        }
    }
}
