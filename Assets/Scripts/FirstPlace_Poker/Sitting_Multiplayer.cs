using UnityEngine;
using Photon.Pun;

namespace GameTest
{
    public class Sitting_Multiplayer : MonoBehaviourPunCallbacks
    {
        [Header("Player States")] 
        public GameObject playerStanding;
        public GameObject playerSitting;
        public GameObject playerSitting1;
        public GameObject playerSitting2;
        public GameObject playerSitting3;
        public GameObject CardsTable;
        public GameObject Blackie;

        [Header("UI Elements")] 
        public GameObject intText; // Texte pour s'asseoir

        private bool interactable = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;

            PhotonView photonView = other.GetComponent<PhotonView>();

            if (photonView != null && photonView.IsMine) 
            {
                intText.SetActive(true);
                interactable = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;

            PhotonView photonView = other.GetComponent<PhotonView>();

            if (photonView != null && photonView.IsMine)
            {
                intText.SetActive(false);
                interactable = false;
            }
        }

        private void Update()
        {
            if (!photonView.IsMine) return; 

            if (interactable && Input.GetKeyDown(KeyCode.E) && !playerSitting.activeSelf)
            {
                intText.SetActive(false);
                photonView.RPC("Sit", RpcTarget.All); 
            }
        }

        [PunRPC] 
        private void Sit()
        {
            playerSitting.SetActive(true);
            playerStanding.SetActive(false);

            if (playerSitting1.activeSelf && playerSitting2.activeSelf && playerSitting3.activeSelf)
            {
                CardsTable.SetActive(true);
                Blackie.SetActive(true);
            }
        }
    }
}
