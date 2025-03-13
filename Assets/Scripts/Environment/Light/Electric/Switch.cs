using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Switch : MonoBehaviourPun, IPunObservable
    {
        private bool isOn = false;
        public AudioSource audioSource;
        public AudioClip switchSound;
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();

            UpdateChildrenState();
        }

        public void ToggleSwitch()
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            ActivateSwitch();
        }


        private void ActivateSwitch()
        {
            isOn = !isOn;
            audioSource.PlayOneShot(switchSound);
            UpdateChildrenState();
        }

        private void UpdateChildrenState()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(isOn);
            }
        }
        
        public bool IsOn()
        {
            return isOn;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isOn);
            }
            else
            {
                bool previousState = isOn;
                isOn = (bool)stream.ReceiveNext();

                if (isOn != previousState)
                {
                    UpdateChildrenState();
                }
            }
        }
    }
}
