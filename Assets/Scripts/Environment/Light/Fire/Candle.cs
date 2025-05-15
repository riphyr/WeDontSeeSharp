using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Candle : MonoBehaviourPun, IPunObservable
    {
        public AudioClip pickupSound, lightSoundLighter, lightSoundMatch;
        public float timeToBurnOut = 30f;
        public int candlesToAdd = 1;
        
        private AudioSource audioSource;
        private PhotonView view;
        private bool isLit = false;
		private bool isUsingLighter = true;
        
        private GameObject newCandle;
        private GameObject oldCandle;
        private GameObject candleLightNew;
        private GameObject candleLightOld;
        
        private Material newCandleMaterialInstance;
        private Material oldCandleMaterialInstance;
        private Renderer newCandleRenderer;
        private Renderer oldCandleRenderer;


        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();

            newCandle = transform.Find("NewCandle")?.gameObject;
            oldCandle = transform.Find("OldCandle")?.gameObject;
            candleLightNew = newCandle?.transform.Find("Candlelight")?.gameObject;
            candleLightOld = oldCandle?.transform.Find("Candlelight")?.gameObject;

            newCandleRenderer = newCandle?.GetComponentInChildren<Renderer>();
            oldCandleRenderer = oldCandle?.GetComponentInChildren<Renderer>();

            if (newCandleRenderer != null)
            {
                Material baseMat = newCandleRenderer.sharedMaterial;
                newCandleMaterialInstance = new Material(baseMat);
                newCandleRenderer.material = newCandleMaterialInstance;
                DisableEmission(newCandleMaterialInstance);
            }

            if (oldCandleRenderer != null)
            {
                Material baseMat = oldCandleRenderer.sharedMaterial;
                oldCandleMaterialInstance = new Material(baseMat);
                oldCandleRenderer.material = oldCandleMaterialInstance;
                DisableEmission(oldCandleMaterialInstance);
            }

            newCandle?.SetActive(true);
            candleLightNew?.SetActive(false);
            oldCandle?.SetActive(false);
            candleLightOld?.SetActive(false);
        }


        public void PickupCandle(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            inventory.AddItem("Candle", candlesToAdd);
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

        public void LightCandle(PlayerInventory inventory)
        {
            if (isLit) 
                return;

            if (inventory.HasItem("Lighter"))
            {
				isUsingLighter = true;
                photonView.RPC("SetCandleLit", RpcTarget.All);
            }
			else if (inventory.HasItem("Match"))
            {
				isUsingLighter = false;
				inventory.RemoveItem("Match", 1);
                photonView.RPC("SetCandleLit", RpcTarget.All);
            }
        }

        [PunRPC]
        private void SetCandleLit()
        {
            isLit = true;
            audioSource.PlayOneShot(isUsingLighter ? lightSoundLighter : lightSoundMatch);
            newCandle.SetActive(true);
            oldCandle.SetActive(false);
            candleLightNew.SetActive(true);
            StartCoroutine(BurnOutTimer());
            
            EnableEmission(newCandleMaterialInstance);
        }

        private IEnumerator BurnOutTimer()
        {
            yield return new WaitForSeconds(timeToBurnOut);
            photonView.RPC("BurntCandle", RpcTarget.All);
        }

        [PunRPC]
        private void BurntCandle()
        {
            newCandle.SetActive(false);
            oldCandle.SetActive(true);
            candleLightOld.SetActive(true);
            candleLightNew.SetActive(false);
            
            EnableEmission(oldCandleMaterialInstance);
            DisableEmission(newCandleMaterialInstance);
        }
        
        private void EnableEmission(Material mat)
        {
            if (mat == null) return;
            mat.EnableKeyword("_EMISSION");
        }

        private void DisableEmission(Material mat)
        {
            if (mat == null) return;
            mat.DisableKeyword("_EMISSION");
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
            }
            else
            {
                transform.position = (Vector3)stream.ReceiveNext();
            }
        }
    }
}
