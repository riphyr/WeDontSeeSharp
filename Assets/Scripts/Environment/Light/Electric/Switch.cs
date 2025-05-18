using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Switch : MonoBehaviourPun, IPunObservable
    {
        [System.Serializable]
        public class LightElement
        {
            public GameObject targetObject;
            public Material emissiveMaterial;
            public GameObject visualIndicator;
            
            [HideInInspector]
            public Color originalEmissionColor;
        }

        [SerializeField] private List<LightElement> lightElements;
        public ElectricLever electricLever;

        private bool isOn = false;
        public AudioSource audioSource;
        public AudioClip switchSound;
        private PhotonView view;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            view = GetComponent<PhotonView>();
            view.OwnershipTransfer = OwnershipOption.Takeover;
            
            foreach (var element in lightElements)
            {
                if (element.emissiveMaterial != null)
                {
                    element.originalEmissionColor = element.emissiveMaterial.GetColor("_EmissionColor");
                }
            }

            isOn = false;
            
            UpdateLightTargets();
        }
        
        public void ToggleSwitch()
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            // Vérification si le levier est activé
            if (electricLever == null || !electricLever.IsActive)
            {
                return;
            }

            ActivateSwitch();
        }

        private void ActivateSwitch()
        {
            isOn = !isOn;
            audioSource.PlayOneShot(switchSound);
            UpdateLightTargets();
        }
        
        [PunRPC]
        public void ForceSwitchOff()
        {
            isOn = false;
            UpdateLightTargets();
        }

        private void UpdateLightTargets()
        {
            foreach (var element in lightElements)
            {
                if (element.targetObject != null)
                {
                    Light[] lights = element.targetObject.GetComponentsInChildren<Light>(true);
                    foreach (Light light in lights)
                    {
                        light.enabled = isOn;
                    }
                }

                if (element.emissiveMaterial != null)
                {
                    if (isOn)
                    {
                        element.emissiveMaterial.EnableKeyword("_EMISSION");
                        element.emissiveMaterial.SetColor("_EmissionColor", element.originalEmissionColor);
                    }
                    else
                    {
                        element.emissiveMaterial.DisableKeyword("_EMISSION");
                        element.emissiveMaterial.SetColor("_EmissionColor", Color.black);
                    }
                }

                if (element.visualIndicator != null)
                {
                    element.visualIndicator.SetActive(isOn);
                }
            }
        }

        public bool IsOn() => isOn;

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
