using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class UVFlashlight : MonoBehaviourPun, IPunObservable, IFlashlightFlicker
    {
        private Light flashlightLight;
        public float maxBattery = 100f;
        public float drain = 10f;
        private float currentBattery;
        private bool isOn = false;
        private bool isEquipped = false;
        private Transform ownerTransform;
        private Camera playerCamera;
        private PhotonView view;
        private AudioSource audioSource;
		private Collider UVflashlightCollider;

        public AudioClip pickupSound;
        public AudioClip switchSound;
        
        public float uvRange = 5f;
        public float uvConeRadius = 0.45f;
        public LayerMask uvLayerMask;
        private Renderer lastHitRenderer = null;

        void Start()
        {
            flashlightLight = transform.Find("light")?.GetComponent<Light>();
            flashlightLight.enabled = false;
            isOn = false;

            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            UVflashlightCollider = GetComponent<BoxCollider>();

            if (view.InstantiationData != null && view.InstantiationData.Length > 0)
            {
                currentBattery = (float)view.InstantiationData[0];
            }
            else
            {
                currentBattery = maxBattery;
            }
        }

        public void PickupFlashlight(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("UVFlashlight", currentBattery);
            audioSource.PlayOneShot(pickupSound);
            photonView.RPC("PlayPickupSound", RpcTarget.Others);
            StartCoroutine(DestroyAfterSound());
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            audioSource.PlayOneShot(pickupSound);
        }

        private IEnumerator DestroyAfterSound()
        {
            yield return new WaitForSeconds(pickupSound.length);
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DestroyForAll()
        {
            Destroy(gameObject);
        }

        void Update()
        {
            if (isEquipped && ownerTransform != null)
            {
                transform.position = ownerTransform.position + ownerTransform.forward * 0.3f + ownerTransform.right * 0.1f + Vector3.up * 0.5f;
                transform.rotation = Quaternion.Euler(playerCamera.transform.eulerAngles.x, ownerTransform.eulerAngles.y, 0f);

                if (view.IsMine)
                {
                    photonView.RPC("SyncFlashlightTransform", RpcTarget.Others, transform.position, transform.rotation, isOn);
                }

                if (isOn)
                {
                    currentBattery -= Time.deltaTime * drain;
                    if (currentBattery <= 0)
                    {
                        OutOfBattery();
                        currentBattery = 0;
                    }
                    else
                    {
                        RevealUVObjects();
                    }
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (flashlightLight == null)
                flashlightLight = transform.Find("light")?.GetComponent<Light>();

            if (flashlightLight == null)
                return;

            Gizmos.color = new Color(1f, 0f, 1f, 0.2f);

            int baseRadialSteps = 60;
            int rings = 7;
            Vector3 lightPos = flashlightLight.transform.position;
            Vector3 lightDir = flashlightLight.transform.forward;

            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float radius = t * uvConeRadius;

                int raysThisRing = Mathf.Max(4, Mathf.RoundToInt(baseRadialSteps * t));
                float angleStep = 360f / raysThisRing;

                for (int i = 0; i < raysThisRing; i++)
                {
                    float angle = i * angleStep;
                    Vector3 offset = (Quaternion.AngleAxis(angle, lightDir) * flashlightLight.transform.right) * radius;
                    Vector3 direction = (lightDir + offset).normalized;

                    Gizmos.DrawRay(lightPos, direction * uvRange);
                }
            }
        }
        
        private void RevealUVObjects()
        {
            if (!view.IsMine || !isOn) return;

            Vector3 lightPos = flashlightLight.transform.position;
            Vector3 lightDir = flashlightLight.transform.forward;

            if (Physics.Raycast(lightPos, lightDir, out RaycastHit centerHit, uvRange, uvLayerMask))
            {
                PhotonView centerTargetView = centerHit.collider.GetComponent<PhotonView>();
                if (centerTargetView != null)
                {
                    Vector3 correctedCenterHit = centerHit.point - centerHit.normal * 0.01f;
                    centerTargetView.RPC("SetUVHit", RpcTarget.AllBuffered, correctedCenterHit);
                }
            }

            int baseRadialSteps = 60;
            int rings = 7;

            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float radius = t * uvConeRadius;

                int raysThisRing = Mathf.Max(4, Mathf.RoundToInt(baseRadialSteps * t));
                float angleStep = 360f / raysThisRing;

                for (int i = 0; i < raysThisRing; i++)
                {
                    float angle = i * angleStep;
                    Vector3 offset = (Quaternion.AngleAxis(angle, lightDir) * flashlightLight.transform.right) * radius;
                    Vector3 direction = (lightDir + offset).normalized;

                    Ray ray = new Ray(lightPos, direction);
                }
            }
        }

        [PunRPC]
        private void SyncFlashlightTransform(Vector3 pos, Quaternion rot, bool state)
        {
            transform.position = pos;
            transform.rotation = rot;
            flashlightLight.enabled = state;
            isOn = state;
        }

        public void AssignOwner(Photon.Realtime.Player player, Transform ownerTransform)
        {
            this.ownerTransform = ownerTransform;
            photonView.TransferOwnership(player);
        }
        
        public void SetCurrentBattery(float level)
        {
            currentBattery = level;
        }
        
        [PunRPC]
        public void SyncBattery(float battery)
        {
            currentBattery = battery;
        }

        public void OutOfBattery()
        {
            isOn = false;
            flashlightLight.enabled = false;
            photonView.RPC("SyncFlashlightState", RpcTarget.All, isOn);
        }

        public bool isOutOfBattery()
        {
            return isEquipped && currentBattery <= 0;
        }

        public void RechargeBattery(PlayerInventory inventory)
        {
            if (inventory == null || !inventory.HasItem("Battery")) 
                return;

            float availableCharge = inventory.GetItemCount("Battery");

            float chargeToUse = Mathf.Min(availableCharge, maxBattery);
            inventory.RemoveItem("Battery", chargeToUse);
            currentBattery += chargeToUse;

            if (isEquipped && !isOn && currentBattery > 0)
                ToggleFlashlight(true);
        }

        public void EquipFlashlight(PlayerInventory inventory, Transform playerTransform)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            ownerTransform = playerTransform;
            playerCamera = ownerTransform.GetComponentInChildren<Camera>();
            isEquipped = true;

            currentBattery = inventory.GetItemCount("UVFlashlight");

			photonView.RPC("SetColliderState", RpcTarget.AllBuffered, false);

            audioSource.PlayOneShot(switchSound);

            if (currentBattery > 0)
            {
                ToggleFlashlight(true);
            }
            else
            {
                ToggleFlashlight(false);
            }

            photonView.RPC("SyncFlashlightState", RpcTarget.All, isOn);
        }

        [PunRPC]
        private void SetColliderState(bool state)
        {
            if (UVflashlightCollider == null)
                UVflashlightCollider = GetComponent<Collider>();

            if (UVflashlightCollider != null)
                UVflashlightCollider.enabled = state;
        }

        [PunRPC]
        private void SyncFlashlightState(bool state)
        {
            isOn = state;
            flashlightLight.enabled = state;
        }

        public void UnequipFlashlight(PlayerInventory inventory)
        {
            StartCoroutine(PlaySwitchSound());
            isEquipped = false;
            inventory.SetItemCount("UVFlashlight", currentBattery);
            ownerTransform = null;

            StartCoroutine(DisableUVObjectsBeforeDestroy());
        }

        private IEnumerator DisableUVObjectsBeforeDestroy()
        {
            yield return new WaitForSeconds(0.1f);
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        
        private void ToggleFlashlight(bool state)
        {
            isOn = state;
            flashlightLight.enabled = state;
            photonView.RPC("SyncFlashlightState", RpcTarget.AllBuffered, isOn);
        }

        private IEnumerator PlaySwitchSound()
        {
            audioSource.PlayOneShot(switchSound);
            yield return new WaitForSeconds(switchSound.length);
        }
        
        public float GetCurrentBattery()
        {
            return currentBattery;
        }
        
        public bool IsEquipped()
        {
            return isEquipped;
        }

		[PunRPC]
		public void EnablePhysicsRPC()
		{
    		Rigidbody rb = GetComponent<Rigidbody>();
    		if (rb == null)
        		rb = gameObject.AddComponent<Rigidbody>();

    		rb.isKinematic = false;
    		rb.useGravity = true;
    		rb.interpolation = RigidbodyInterpolation.Interpolate;
    		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    		BoxCollider col = GetComponent<BoxCollider>();
    		if (col != null)
        		col.enabled = true;
		}


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(isOn);
                stream.SendNext(currentBattery);
            }
            else
            {
                transform.position = (Vector3)stream.ReceiveNext();
                transform.rotation = (Quaternion)stream.ReceiveNext();
                isOn = (bool)stream.ReceiveNext();
                currentBattery = (float)stream.ReceiveNext();

                flashlightLight.enabled = isOn;
            }
        }
        
        public void TriggerFlicker()
        {
            if (isOn && !isOutOfBattery())
                StartCoroutine(FlickerRoutine());
        }

        private IEnumerator FlickerRoutine()
        {
            for (int i = 0; i < 5; i++)
            {
                flashlightLight.enabled = !flashlightLight.enabled;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
            flashlightLight.enabled = isOn;
        }
    }
}