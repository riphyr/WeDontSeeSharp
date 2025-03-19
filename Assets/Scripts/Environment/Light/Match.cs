using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Match : MonoBehaviourPun, IPunObservable
    {
        public float burnDuration = 5f;
        private AudioSource audioSource;
        private PhotonView view;
        public AudioClip igniteSound;

        private Transform ownerTransform;
        private bool isBurning = false;

        void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
            
            transform.localScale = Vector3.one * 0.2f;
        }

        
        public void AssignOwner(Photon.Realtime.Player player, Transform ownerTransform)
        {
            this.ownerTransform = ownerTransform;
            photonView.TransferOwnership(player);
        }

        public void IgniteMatch()
        {
            if (isBurning) return;
            isBurning = true;

            if (view.IsMine)
            {
                if (audioSource != null && igniteSound != null)
                {
                    audioSource.PlayOneShot(igniteSound);
                }
            }

            StartCoroutine(BurnOut());
        }

        void Update()
        {
            if (isBurning && ownerTransform != null)
            {
                transform.position = ownerTransform.position + ownerTransform.forward * 0.3f + ownerTransform.right * 0.1f + Vector3.up * 0.6f;
                transform.rotation = Quaternion.Euler(0f, ownerTransform.eulerAngles.y, 0f);
            }
        }
        
        private IEnumerator BurnOut()
        {
            yield return new WaitForSeconds(burnDuration);

            if (view.IsMine)
            {
                view.RPC("DestroyMatch_RPC", RpcTarget.All);
            }
        }

        [PunRPC]
        private void DestroyMatch_RPC()
        {
            Destroy(gameObject);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                transform.position = (Vector3)stream.ReceiveNext();
                transform.rotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}