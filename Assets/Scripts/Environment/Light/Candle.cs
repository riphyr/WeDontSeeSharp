using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Candle : MonoBehaviourPun
    {
        public AudioClip pickupSound, lightSoundLighter, lightSoundMatch;
        public float timeToBurnOut = 30f;
        
        private AudioSource audioSource;
        private PhotonView view;
        private bool isLit = false;
		private bool isUsingLighter = true;
        
        private GameObject newCandle;
        private GameObject oldCandle;
        private GameObject candleLightNew;
        private GameObject candleLightOld;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
            
            newCandle = transform.Find("NewCandle")?.gameObject;
            oldCandle = transform.Find("OldCandle")?.gameObject;
            candleLightNew = newCandle?.transform.Find("Candlelight")?.gameObject;
            candleLightOld = oldCandle?.transform.Find("Candlelight")?.gameObject;
            
            oldCandle.SetActive(false);
            candleLightOld.SetActive(false);
            candleLightNew.SetActive(false);
        }

        public void PickupCandle(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.RequestOwnership();
            }

            inventory.AddItem("Candle", 1);
            photonView.RPC("PlayPickupSound", RpcTarget.All);
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            StartCoroutine(PlaySoundAndDisable());
        }

        private IEnumerator PlaySoundAndDisable()
        {
            audioSource.PlayOneShot(pickupSound);
            yield return new WaitForSeconds(pickupSound.length);
            photonView.RPC("DisableForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DisableForAll()
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
        }
    }
}
