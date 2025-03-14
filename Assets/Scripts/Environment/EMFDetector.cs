using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

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
        
        public LayerMask blockLayers;       // ❌ Bloque totalement le signal
        public LayerMask wallLayers;        // ⛔ Réduit sévèrement (~30% passe)
        public LayerMask doorLayers;        // 🚪 Réduit moyennement (~60% passe)
        public LayerMask thinObjectLayers;  // 🪟 Réduction faible (~85% passe)
        public LayerMask ignoredLayers;     // 🚷 Couches ignorées (ex: joueur)

        private AudioSource audioSource;
        private PhotonView view;
        private Transform ownerTransform;
        private bool isActive = false;
        private int currentLevel = 0;
        private float nextBeepTime = 0f;

        void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            SetLEDLevel(0);
            Debug.Log("🚀 EMF Detector initialized. LEDs set to 0.");
        }

        public void PickupEMF(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("EMFDetector");
            photonView.RPC("PlayPickupSound", RpcTarget.All);
            Debug.Log("🎤 EMF Detector picked up!");
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
            Debug.Log($"🔁 EMF Detector toggled. Active: {isActive}");

            if (!isActive)
            {
                SetLEDLevel(0);
                Debug.Log("❌ EMF Detector turned off. LEDs set to 0.");
            }
        }

        void Update()
        {
            if (!isActive) return;

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

            Debug.Log($"🛠 Checking for entities... Found {colliders.Length} objects.");

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Ghost"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    float signalStrength = GetSignalStrength(collider.transform, distance);
            
                    if (signalStrength > 0)
                    {
                        Debug.Log($"👻 Ghost detected at {distance}m with signal strength {signalStrength}");

                        // 🟢 Détection brute en fonction de la distance
                        float rawLevel = 0f;
                        if (distance < 3f) rawLevel = 5;
                        else if (distance < 5f) rawLevel = 4;
                        else if (distance < 7f) rawLevel = 3;
                        else if (distance < 10f) rawLevel = 2;
                        else if (distance < 15f) rawLevel = 1;

                        // 🛠 Appliquer une atténuation **douce** du signalStrength
                        float adjustedSignal = Mathf.Sqrt(signalStrength); // **Douce diminution**
                        // ou 
                        // float adjustedSignal = Mathf.Lerp(0.5f, 1.0f, signalStrength);  // **Autre option de lissage**
                
                        detectedLevel = Mathf.RoundToInt(rawLevel * adjustedSignal);

                        // 🔥 Toujours garantir un niveau minimal si le signal n'est pas bloqué
                        detectedLevel = Mathf.Clamp(detectedLevel, 1, 5);

                        Debug.Log($"🔧 Final EMF Level after signal reduction: {detectedLevel}");
                    }
                }
            }

            return detectedLevel;
        }

        private float GetSignalStrength(Transform ghost, float distance)
        {
            Vector3 direction = (ghost.position - transform.position).normalized;
            Debug.DrawRay(transform.position, direction * distance, Color.cyan, 1.0f);

            Vector3 currentPos = transform.position;
            float signalStrength = 1.0f;
            float remainingDistance = distance;
            bool reachedGhost = false;

            while (remainingDistance > 0)
            {
                RaycastHit hit;
                if (Physics.Raycast(currentPos, direction, out hit, remainingDistance))
                {
                    if (hit.transform == ghost) // 🎯 On atteint le fantôme
                    {
                        reachedGhost = true;
                        break;
                    }

                    if (((1 << hit.collider.gameObject.layer) & blockLayers) != 0)
                    {
                        Debug.Log($"❌ Signal bloqué par {hit.transform.name} !");
                        return 0f;
                    }
                    else if (((1 << hit.collider.gameObject.layer) & wallLayers) != 0)
                    {
                        Debug.Log($"⛔ Mur détecté ({hit.transform.name}) : signal réduit de 40%");
                        signalStrength *= 0.6f;
                    }
                    else if (((1 << hit.collider.gameObject.layer) & doorLayers) != 0)
                    {
                        Debug.Log($"🚪 Porte détectée ({hit.transform.name}) : signal réduit de 10%");
                        signalStrength *= 0.9f;
                    }
                    else if (((1 << hit.collider.gameObject.layer) & thinObjectLayers) != 0)
                    {
                        Debug.Log($"🪟 Objet fin détecté ({hit.transform.name}) : signal réduit de 1%");
                        signalStrength *= 0.99f;
                    }

                    currentPos = hit.point + (direction * 0.1f);
                    remainingDistance -= hit.distance;
                }
                else
                {
                    reachedGhost = true;
                    break;
                }
            }

            return reachedGhost ? signalStrength : 0f;
        }

        [PunRPC]
        private void RPC_SetLEDLevel(int level)
        {
            SetLEDLevel(level);
            Debug.Log($"💡 LEDs updated. New level: {level}");
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
                Debug.Log("🔊 Beep!");
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

        void LateUpdate()
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