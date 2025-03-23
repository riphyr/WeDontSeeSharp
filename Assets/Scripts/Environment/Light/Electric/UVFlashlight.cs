using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class UVFlashlight : MonoBehaviourPun, IPunObservable
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
        private List<Renderer> uvObjectsInLight = new List<Renderer>();

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
                Debug.Log($"[DEBUG][UV] Batterie reçue via InstantiationData : {currentBattery}");
            }
            else
            {
                currentBattery = maxBattery;
                Debug.Log("[DEBUG][UV] Aucune InstantiationData, batterie full.");
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
                        CheckForUVObjects();
                    }
                }
            }
        }
        
        private void CheckForUVObjects()
        {
            List<Renderer> currentlyLitObjects = new List<Renderer>();

            int raysPerRow = 15;
            int rows = 12;
            float angleStep = uvConeRadius / raysPerRow;
            Vector3 lightDirection = flashlightLight.transform.forward;

            for (int i = -rows / 2; i <= rows / 2; i++)
            {
                for (int j = -raysPerRow / 2; j <= raysPerRow / 2; j++)
                {
                    Vector3 offset = (flashlightLight.transform.right * j * angleStep) + (flashlightLight.transform.up * i * angleStep);
                    Vector3 rayDirection = (lightDirection + offset).normalized;
                    Ray ray = new Ray(flashlightLight.transform.position, rayDirection);
                    RaycastHit hit;

                    Debug.DrawRay(ray.origin, ray.direction * uvRange, Color.magenta, 0.1f);

                    if (Physics.Raycast(ray, out hit, uvRange, uvLayerMask))
                    {
                        Renderer objRenderer = hit.collider.GetComponent<Renderer>();
                        PhotonView objPhotonView = hit.collider.GetComponent<PhotonView>();

                        if (objRenderer != null && objPhotonView != null)
                        {
                            currentlyLitObjects.Add(objRenderer);

                            if (!uvObjectsInLight.Contains(objRenderer))
                            {
                                uvObjectsInLight.Add(objRenderer);

                                photonView.RPC("SetUVIntensity_RPC", RpcTarget.AllBuffered, objPhotonView.ViewID, 2f);
                            }
                        }
                    }
                }
            }

            foreach (Renderer obj in uvObjectsInLight.ToArray())
            {
                if (!currentlyLitObjects.Contains(obj))
                {
                    uvObjectsInLight.Remove(obj);
                    PhotonView objPhotonView = obj.GetComponent<PhotonView>();

                    if (objPhotonView != null)
                    {
                        photonView.RPC("SetUVIntensity_RPC", RpcTarget.AllBuffered, objPhotonView.ViewID, 0f);
                    }
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (flashlightLight == null) return;

            Gizmos.color = new Color(1f, 0f, 1f, 0.2f);
            Vector3 lightPos = flashlightLight.transform.position;
            Vector3 lightDir = flashlightLight.transform.forward;

            for (int i = -5; i <= 5; i++)
            {
                for (int j = -5; j <= 5; j++)
                {
                    Vector3 direction = (lightDir + (flashlightLight.transform.right * i * 0.08f) + (flashlightLight.transform.up * j * 0.08f)).normalized;
                    Gizmos.DrawRay(lightPos, direction * uvRange);
                }
            }
        }

        private void ResetLastObject()
        {
            foreach (Renderer objRenderer in uvObjectsInLight)
            {
                if (objRenderer != null)
                {
                    PhotonView objPhotonView = objRenderer.GetComponent<PhotonView>();
                    if (objPhotonView != null)
                    {
                        photonView.RPC("SetUVIntensity_RPC", RpcTarget.AllBuffered, objPhotonView.ViewID, 0f);
                    }
                }
            }

            uvObjectsInLight.Clear();
        }
        
        [PunRPC]
        private void SetUVIntensity_RPC(int objectID, float value)
        {
            PhotonView objView = PhotonView.Find(objectID);
            if (objView == null) 
            {
                return;
            }

            Renderer objRenderer = objView.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material.SetFloat("_UVIntensity", value);
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
            ResetLastObject();
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
            ResetLastObject();
            yield return new WaitForSeconds(0.1f);
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        
        private void ToggleFlashlight(bool state)
        {
            isOn = state;
            flashlightLight.enabled = state;
            photonView.RPC("SyncFlashlightState", RpcTarget.AllBuffered, isOn);

            if (!isOn) 
            {
                ResetLastObject();
            }
        }

        private IEnumerator PlaySwitchSound()
        {
            audioSource.PlayOneShot(switchSound);
            yield return new WaitForSeconds(switchSound.length);
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
    }
}
