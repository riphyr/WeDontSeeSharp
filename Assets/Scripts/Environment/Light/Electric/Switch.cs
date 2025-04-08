using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Switch : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private List<GameObject> lightTargets;
        private bool isOn = false;
        public AudioSource audioSource;
        public AudioClip switchSound;
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
            view.OwnershipTransfer = OwnershipOption.Takeover;

            UpdateLightTargets();
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
            UpdateLightTargets();
        }

        private void UpdateLightTargets()
        {
            foreach (GameObject target in lightTargets)
            {
                if (target == null) continue;

                Light[] lights = target.GetComponentsInChildren<Light>(true);
                foreach (Light light in lights)
                {
                    light.enabled = isOn;
                }
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
                    UpdateLightTargets();
                }
            }
        }
    }
}
