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
        public AudioSource burningSource;
        public AudioClip boilingSound;
        public AudioSource boilingSource;
        public AudioClip steamSound;
        public AudioSource steamSource;
        public AudioClip inputSound;
        public AudioClip outputSound;
        public AudioClip pickupSound;
        public AudioClip explosionSound;
        public AudioSource mainSource;

        [Header("FLAT BOTTOM FLASK")] 
        public GameObject Input1_LemonJuice;
        public GameObject Input1_Bleach;
        public GameObject Input1_LemonJuice_Particles;
        public GameObject Input1_Bleach_Particles;
        
        [Header("CONDENSER")] 
        public GameObject Condenser_LemonJuice;
        public GameObject Condenser_Bleach;
        
        [Header("BOILING FLASK")] 
        public GameObject Input2_RedWine;
        public GameObject Input2_WD40;
        public GameObject Input2_RedWine_Particles;
        public GameObject Input2_WD40_Particles;
        
        [Header("BUNSEN BURNER")] 
        public GameObject Flame;
        
        [Header("OUTPUT FLASK")] 
        public GameObject Output_Solvent;
        public GameObject Output_InertSolution;
        public GameObject Output_ExplosionSolution;
        public GameObject Output_Explosion_Particles;

        private string currentFlatBottomSolution = "";
        private string currentBoilingSolution = "";
        private string currentOutputSolution = "";

        private bool isFlatBottomFilled = false;
        private bool isBoilingFilled = false;
        private bool isOutputFilled = false;
        private bool isBurning = false;
        
        private PhotonView view;

        void Start()
        {
            view = GetComponent<PhotonView>();

            AudioSource[] allSources = GetComponents<AudioSource>();
            if (allSources.Length >= 4)
            {
                burningSource = allSources[0];
                boilingSource = allSources[1];
                steamSource = allSources[2];
                mainSource = allSources[3];
            }
            else
            {
                Debug.LogError("[ChemistryStation] Pas assez d'AudioSource sur l'objet !");
            }

            if (burningSource != null) {
                burningSource.clip = burningSound;
                burningSource.loop = true;
                burningSource.volume = 0.3f;
            }

            if (boilingSource != null) {
                boilingSource.clip = boilingSound;
                boilingSource.loop = true;
                boilingSource.volume = 0.1f;
            }

            if (steamSource != null) {
                steamSource.clip = steamSound;
                steamSource.loop = true;
                steamSource.volume = 0.1f;
            }

            ResetAllVisuals();
            
            if (Output_Explosion_Particles == null)
            {
                Output_Explosion_Particles = GameObject.Find("Output_Explosion_Particles");
                if (Output_Explosion_Particles != null)
                    Debug.LogWarning("[Alchemy] Output_Explosion_Particles assigné dynamiquement via Find.");
                else
                    Debug.LogError("[Alchemy] Output_Explosion_Particles introuvable dans la scène !");
            }
        }

        private void ResetAllVisuals()
        {
            if (Input1_LemonJuice != null) Input1_LemonJuice.SetActive(false);
            if (Input1_Bleach != null) Input1_Bleach.SetActive(false);
            if (Input1_LemonJuice_Particles != null) Input1_LemonJuice_Particles.SetActive(false);
            if (Input1_Bleach_Particles != null) Input1_Bleach_Particles.SetActive(false);
            if (Condenser_LemonJuice != null) Condenser_LemonJuice.SetActive(false);
            if (Condenser_Bleach != null) Condenser_Bleach.SetActive(false);
            if (Input2_RedWine != null) Input2_RedWine.SetActive(false);
            if (Input2_WD40 != null) Input2_WD40.SetActive(false);
            if (Input2_RedWine_Particles != null) Input2_RedWine_Particles.SetActive(false);
            if (Input2_WD40_Particles != null) Input2_WD40_Particles.SetActive(false);
            if (Flame != null) Flame.SetActive(false);
            if (Output_Solvent != null) Output_Solvent.SetActive(false);
            if (Output_InertSolution != null) Output_InertSolution.SetActive(false);
            if (Output_ExplosionSolution != null) Output_ExplosionSolution.SetActive(false);

            if (isOutputFilled)
            {
                burningSource.Stop();
                boilingSource.Stop();
                steamSource.Stop();
            }
        }

        public void InteractFlatBottomFlask(string solution, PlayerInventory inventory)
        {
            if (isFlatBottomFilled) return;

            inventory.RemoveItem(solution, 1);
            photonView.RPC("RPC_InteractFlatBottomFlask", RpcTarget.AllBuffered, solution);
        }

        [PunRPC]
        private void RPC_InteractFlatBottomFlask(string solution)
        {
            isFlatBottomFilled = true;
            currentFlatBottomSolution = solution;

            if (solution == "Lemon juice")
            {
                Input1_LemonJuice.SetActive(true);
                Input1_LemonJuice_Particles.SetActive(true);
            }
            else if (solution == "Bleach")
            {
                Input1_Bleach.SetActive(true);
                Input1_Bleach_Particles.SetActive(true);
            }

            mainSource.PlayOneShot(inputSound);
            StartCoroutine(ActivateCondenser(solution));
        }

        private IEnumerator ActivateCondenser(string solution)
        {
            yield return new WaitForSeconds(0.5f);
            if (solution == "Lemon juice") Condenser_LemonJuice.SetActive(true);
            else if (solution == "Bleach") Condenser_Bleach.SetActive(true);
        }
        
        public void InteractBoilingFlask(string solution, PlayerInventory inventory)
        {
            if (isBoilingFilled) return;

            inventory.RemoveItem(solution, 1);
            photonView.RPC("RPC_InteractBoilingFlask", RpcTarget.AllBuffered, solution);
        }

        [PunRPC]
        private void RPC_InteractBoilingFlask(string solution)
        {
            isBoilingFilled = true;
            currentBoilingSolution = solution;
            mainSource.PlayOneShot(inputSound, 2f);

            if (solution == "WD40") Input2_WD40.SetActive(true);
            else if (solution == "Red wine") Input2_RedWine.SetActive(true);
        }

        public void InteractBunsenBurner()
        {
            if (!isBoilingFilled || isBurning) return;
            photonView.RPC("RPC_InteractBunsenBurner", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RPC_InteractBunsenBurner()
        {
            Flame.SetActive(true);
            isBurning = true;
            burningSource.Play();
            StartCoroutine(StartReaction());
        }

        private IEnumerator StartReaction()
        {
            boilingSource.Play();
            steamSource.Play();

            if (currentBoilingSolution == "WD40") Input2_WD40_Particles.SetActive(true);
            else if (currentBoilingSolution == "Red wine") Input2_RedWine_Particles.SetActive(true);

            yield return new WaitForSeconds(4f);
            
            if (isFlatBottomFilled && !isOutputFilled)
            {
                photonView.RPC("RPC_SetOutput", RpcTarget.AllBuffered, DetermineOutputSolution());
            }
        }

        private string DetermineOutputSolution()
        {
            if (currentFlatBottomSolution == "Bleach" && currentBoilingSolution == "WD40") return "Explosion";
            if (currentFlatBottomSolution == "Lemon juice" && currentBoilingSolution == "Red wine") return "Inert Solution";
            if (currentFlatBottomSolution == "Bleach" && currentBoilingSolution == "Red wine") return "Explosion";
            if (currentFlatBottomSolution == "Lemon juice" && currentBoilingSolution == "WD40") return "Solvent";
            return "";
        }

        [PunRPC]
        private void RPC_SetOutput(string solution)
        {
            isOutputFilled = true;
            currentOutputSolution = solution;

            if (mainSource == null)
            {
                Debug.LogError("[Alchemy] mainSource est NULL sur ce client !");
            }

            if (outputSound == null)
            {
                Debug.LogError("[Alchemy] outputSound est NULL !");
            }

            if (mainSource != null && outputSound != null)
                mainSource.PlayOneShot(outputSound, 0.6f);

            if (solution == "Explosion")
            {
                Output_ExplosionSolution.SetActive(true);

                if (Output_Explosion_Particles == null)
                {
                    Debug.LogError("[Alchemy] Output_Explosion_Particles est NULL !");
                    return;
                }

                Debug.Log("[Alchemy] Déclenchement des particules d’explosion...");

                foreach (ParticleSystem ps in Output_Explosion_Particles.GetComponentsInChildren<ParticleSystem>(true))
                {
                    Debug.Log($"[Alchemy] Play() sur {ps.gameObject.name}");
                    ps.Play(true);
                }

                mainSource.PlayOneShot(explosionSound, 1f);
            }
            else if (solution == "Inert Solution") Output_InertSolution.SetActive(true);
            else if (solution == "Solvent") Output_Solvent.SetActive(true);
        }

        public void CollectOutput(PlayerInventory inventory)
        {
            if (!isOutputFilled) return;

            mainSource.PlayOneShot(pickupSound, 0.6f);
            inventory.AddItem(currentOutputSolution);
            photonView.RPC("RPC_ResetStation", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void RPC_ResetStation()
        {
            ResetAllVisuals();
            isFlatBottomFilled = false;
            isBoilingFilled = false;
            isOutputFilled = false;
            isBurning = false;
        }

        public bool IsFlatBottomFilled()
        {
            return isFlatBottomFilled;
        }

        public bool IsBoilingFilled()
        {
            return isBoilingFilled;
        }
        
        public bool IsBurning()
        {
            return isBurning;
        }

        public bool IsOutputFilled()
        {
            return isOutputFilled;
        }
    }
}
