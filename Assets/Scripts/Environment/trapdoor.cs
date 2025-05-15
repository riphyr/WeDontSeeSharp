using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;

namespace InteractionScripts
{
    public class Trapdoor : MonoBehaviourPunCallbacks
    {
        [Header("UI Elements")] public GameObject Text; // UI pour afficher le texte 
        public TMP_Text interactionText; // Composant TMP_Text affichant la touche prédefinie

        [Header("Interaction Settings")] public float interactionDistance = 3f; // Distance maximale d'interaction
        public LayerMask interactableLayer; // Masque de calque pour les objets interactifs

        public float speed = 2f;
        public Transform solGauche, posGauche;

        private bool isMoving = false;
        [SerializeField] private bool isActivated = false; // Synchronisé via RPC

        //private KeyCode useKey; // Touche définie par le joueur

        void Start()
        {
            if (photonView == null)
            {
                Debug.LogError("❌ photonView est NULL ! Vérifie que ce GameObject a bien un PhotonView.");
            }
            else
            {
                Debug.Log("✅ photonView trouvé avec ViewID : " + photonView.ViewID);
            }
            // Récupération de la touche "Use" définie par le joueur

            //useKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("UseKey", "E")); 

            // Sécuriser la désactivation de l'UI
            //if (Text != null)
            //Text.SetActive(false);
            //else
            //Debug.LogWarning("⚠️ L'objet Text n'est pas assigné dans l'Inspector !");
        }

        /*void Update()
        {

            if (isActivated) return; // Désactive l'interaction si la trappe est déjà ouverte

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Raycast depuis la caméra
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
            {
                 Debug.Log($"{hit.transform.name}");
                if (hit.collider.gameObject == gameObject) // Vérifie si l'objet touché est bien la trappe
                {
                    if (Text != null && interactionText != null)
                    {
                         Debug.Log("okkkkkkkkkkkkkkkkkkkkkkk");
                        Text.SetActive(true);
                        interactionText.text = "[" + useKey.ToString() + "] Utiliser";
                        //Debug.Log("UI affichée avec la touche : " + useKey.ToString());
                    }
                    if (Input.GetKeyDown(KeyCode.E) && !isMoving)
                    {
                        if (Text != null) Text.SetActive(false);
                        photonView.RPC("MoveTrapdoor", RpcTarget.AllBuffered); // RPC pour tous les joueurs
                    }
                }
            }
            else
            {
                if (Text != null) Text.SetActive(false); // Cache l'UI si on ne regarde plus la trappe
            }
        }*/
        public bool CanInteract()
        {
            return !isActivated && !isMoving;
        }

        [PunRPC]
        public void MoveTrapdoor()
        {
            if (!isMoving)
            {
                isActivated = true; // Empêche toute autre interaction
                StartCoroutine(TrapCoroutine());
            }
        }

        IEnumerator TrapCoroutine()
        {
            isMoving = true;

            float duration = 5f;
            float elapsedTime = 0f;
            Vector3 startPos = solGauche.position;
            Vector3 endPos = posGauche.position;

            while (elapsedTime < duration)
            {
                solGauche.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            solGauche.position = endPos;
            isMoving = false;
        }
    }
}
