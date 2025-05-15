using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class EMFDetector : MonoBehaviourPun, IPunObservable
    {
        [Header("References")]
        public Transform[] ledLights;
        public AudioClip pickupSound;
        public AudioClip beepSound;
        public float detectionRange = 15f;

        [Header("Layers")]
        public LayerMask blockLayers;
        public LayerMask wallLayers;
        public LayerMask doorLayers;
        public LayerMask thinObjectLayers;
        
        [Header("Behavior Settings")]
        public bool disableDetection = false;

        private AudioSource audioSource;
        private PhotonView view;
        private Transform ownerTransform;

        private Vector3 networkPosition;
        private Quaternion networkRotation;

        private bool isTaken = false;
        private int currentLevel = 0;
        private float nextBeepTime = 0f;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            SetLEDLevel(0);
        }

        public void AssignOwner(Photon.Realtime.Player player, Transform newOwnerTransform)
        {
            this.ownerTransform = newOwnerTransform;
            photonView.TransferOwnership(player);

            photonView.RPC("RPC_SetOwnerTransform", RpcTarget.OthersBuffered, player.ActorNumber);
        }

        [PunRPC]
        private void RPC_SetOwnerTransform(int playerID)
        {
            Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerID);
            if (player != null)
            {
                ownerTransform = player.TagObject as Transform;
            }
        }

        public void ShowEMF(bool show)
        {
            isTaken = show;

            if (!show)
            {
                photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        private void DestroyForAll()
        {
            Destroy(gameObject);
        }

        public void PickupEMF(PlayerInventory inventory)
        {
            if (!view.IsMine)
                view.TransferOwnership(PhotonNetwork.LocalPlayer);

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

        private void SetLEDLevel(int level)
        {
            for (int i = 0; i < ledLights.Length; i++)
                ledLights[i].gameObject.SetActive(i < level);
        }

        void Update()
        {
			if (photonView.IsMine)
            {
                if (ownerTransform != null)
                {
                    transform.position = ownerTransform.position + ownerTransform.forward * 0.2f + ownerTransform.right * 0.1f + Vector3.up * 0.6f;
                    transform.rotation = Quaternion.Euler(0f, ownerTransform.eulerAngles.y - 180f, 0f);
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            }
            
            if (disableDetection) return;
	
            int newLevel = DetectEntities();
            if (newLevel != currentLevel)
            {
            	currentLevel = newLevel;
				view.RPC("RPC_SetLEDLevel", RpcTarget.All, currentLevel);
				view.RPC("RPC_PlayEMFBeep", RpcTarget.All);
			}
			if (currentLevel == 5 && Time.time >= nextBeepTime)
			{
				nextBeepTime = Time.time + GetBeepFrequency(5);
				view.RPC("RPC_PlayEMFBeep", RpcTarget.All);
			}
           
        }

        private int DetectEntities()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);
            int detectedLevel = 0;

            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Ghost"))
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    (float signal, bool hitWall, bool hitDoor) = GetSignalStrength(col.transform, dist);

                    if (signal > 0)
                    {
                        float rawLevel = dist switch
                        {
                            < 3f => 5,
                            < 5f => 4,
                            < 7f => 3,
                            < 10f => 2,
                            < 15f => 1,
                            _ => 0
                        };

                        int level = Mathf.RoundToInt(rawLevel * Mathf.Sqrt(signal));
                        if (hitWall)
                            level = Mathf.Min(level, 3);
                        else if (hitDoor)
                            level = Mathf.Min(level, 4);

                        detectedLevel = Mathf.Clamp(level, 1, 5);
                    }
                }
            }
            return detectedLevel;
        }

        private (float signal, bool hitWall, bool hitDoor) GetSignalStrength(Transform ghost, float distance)
        {
            Vector3 dir = (ghost.position - transform.position).normalized;
            Vector3 current = transform.position;
            float signal = 1.0f;
            float remaining = distance;

            bool wallHit = false;
            bool doorHit = false;

            while (remaining > 0)
            {
                if (Physics.Raycast(current, dir, out RaycastHit hit, remaining))
                {
                    if (hit.transform == ghost)
                        return (signal, wallHit, doorHit);

                    int layer = hit.collider.gameObject.layer;

                    if (((1 << layer) & blockLayers) != 0)
                        return (0f, wallHit, doorHit);

                    if (((1 << layer) & wallLayers) != 0)
                    {
                        signal *= 0.6f;
                        wallHit = true;
                    }
                    else if (((1 << layer) & doorLayers) != 0)
                    {
                        signal *= 0.9f;
                        doorHit = true;
                    }
                    else if (((1 << layer) & thinObjectLayers) != 0)
                    {
                        signal *= 0.99f;
                    }

                    current = hit.point + dir * 0.1f;
                    remaining -= hit.distance;
                }
                else
                {
                    break;
                }
            }

            return (signal, wallHit, doorHit);
        }

        [PunRPC]
        private void RPC_SetLEDLevel(int level) => SetLEDLevel(level);

        [PunRPC]
        private void RPC_PlayEMFBeep()
        {
			if (beepSound != null)
			{
            	audioSource.PlayOneShot(beepSound);
        	}
		}

        private float GetBeepFrequency(int level) => level switch
        {
            1 => 2f,
            2 => 1.5f,
            3 => 1f,
            4 => 0.6f,
            5 => 0.3f,
            _ => 2f
        };

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}