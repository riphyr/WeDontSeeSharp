using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class ChemistryStation : MonoBehaviourPun
    {
        [Header("SOUNDS")]
        public AudioClip burningSound;
        public AudioClip boilingSound;
        public AudioClip steamSound;
        public AudioClip dripSound;

        [Header("FLAT BOTTOM FLASK")] 
        public GameObject Input1_CitricAcid;
        public GameObject Input1_Bleach;
        public GameObject Input1_CitricAcid_Particles;
        public GameObject Input1_Bleach_Particles;
        
        [Header("CONDENSER")] 
        public GameObject Condenser_CitricAcid;
        public GameObject Condenser_Bleach;
        
        [Header("BOILING FLASK")] 
        public GameObject Input2_Vinegar;
        public GameObject Input2_RustRemover;
        public GameObject Input2_Vinegar_Particles;
        public GameObject Input2_RustRemover_Particles;
        
        [Header("BUNSEN BURNER")] 
        public GameObject Flame;
        
        [Header("OUTPUT FLASK")] 
        public GameObject Output_Solvent;
        public GameObject Output_InertSolution;
        public GameObject Output_ExplosionSolution;
        public GameObject Output_Explosion_Particles;

        private bool isEmptyFlatBottomFlask = true;
        private bool isEmptyBoilingFlask = true;
        private bool isEmptyOutputFlask = true;
        private bool isBurning = false;
        
        private AudioSource audioSource;
        private PhotonView view;
        
        void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
        }

        public void InteractFlatBottomFlask(PlayerInventory inventory)
        {
            
        }
    }
}