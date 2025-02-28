using UnityEngine;

namespace GameTest
{
    public class Sitting_SinglePlayer : MonoBehaviour
    {
        [Header("Player States")] 
        public GameObject playerStanding;
        public GameObject playerSitting;
        public GameObject playerSitting1;
        public GameObject playerSitting2;
        public GameObject playerSitting3;
        //public GameObject CardsTable;
        //public GameObject Blackie;

        [Header("UI Elements")] 
        public GameObject intText; 

        private bool interactable = false;

        private void OnTriggerEnter(Collider other)
        {
            //if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;

            intText.SetActive(true);
            interactable = true;
        }

        private void OnTriggerExit(Collider other)
        {
            //if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;
            intText.SetActive(false);
            interactable = false;
        }

        private void Update()
        {
            if (interactable && Input.GetKeyDown(KeyCode.E) && !playerSitting.activeSelf)
            {
                intText.SetActive(false);
                Sit();
            }
        }

        private void Sit()
        {
            playerStanding.SetActive(false);
            playerSitting.SetActive(true);
            
            /*
            if (playerSitting1.activeSelf && playerSitting2.activeSelf && playerSitting3.activeSelf)
            {
                CardsTable.SetActive(true);
                Blackie.SetActive(true);
             */
        }
    }
}
