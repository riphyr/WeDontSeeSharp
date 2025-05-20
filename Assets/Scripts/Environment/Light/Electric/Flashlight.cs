using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Flashlight : MonoBehaviourPun, IPunObservable, IFlashlightFlicker
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
        private Collider flashlightCollider;

        public AudioClip pickupSound;
        public AudioClip switchSound;

        void Start()
        {
            flashlightLight = transform.Find("light")?.GetComponent<Light>();
            flashlightLight.enabled = false;
            isOn = false;

            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            flashlightCollider = GetComponent<Collider>();

            if (view.InstantiationData != null && view.InstantiationData.Length > 0)
            {
                currentBattery = (float)view.InstantiationData[0];
            }
            else
            {
                currentBattery = maxBattery;
            }
        }
        
        private IEnumerator GetStartBattery()
        {
            yield return new WaitForSeconds(0.5f);

            if (ownerTransform == null)
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

            inventory.AddItem("Flashlight", currentBattery);
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
                transform.rotation = Quaternion.Euler(playerCamera.transform.eulerAngles.x -5f, ownerTransform.eulerAngles.y - 2.5f, -2.4f);
                
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

            photonView.RPC("SetColliderState", RpcTarget.AllBuffered, false);

            audioSource.PlayOneShot(switchSound);

            if (currentBattery > 0)
                ToggleFlashlight(true);
            else
                ToggleFlashlight(false);

            photonView.RPC("SyncFlashlightState", RpcTarget.All, isOn);
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
            inventory.SetItemCount("Flashlight", currentBattery);
            ownerTransform = null;

            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }
        
        [PunRPC]
        private void SetColliderState(bool state)
        {
            flashlightCollider.enabled = state;
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

    		Collider col = GetComponent<Collider>();
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