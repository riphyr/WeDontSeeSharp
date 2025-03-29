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
