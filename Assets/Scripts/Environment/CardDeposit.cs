using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;

namespace InteractionScripts
{
    public class CardDeposit : MonoBehaviour
    {
        
        public GameObject cardPrefab; // Prefab de la carte à instancier sur la table
        public Transform tablePosition; // Position où les cartes seront déposées
        private bool isNearTable = false;
        public GameObject cards;

        public void Deposit()
        {
            Debug.Log("Deposit called");
            if (isNearTable && CardPickup.collectedCards.Count > 0 && Input.GetKeyDown(KeyCode.E))
            {
                CardData cardToPlace = CardPickup.collectedCards[0]; // Prend la première carte de la liste
                photonView.RPC("SpawnCardOnTable", RpcTarget.AllBuffered, cardToPlace.name);
                CardPickup.collectedCards.RemoveAt(0); // Retire la carte de l'inventaire
            }
        }
        [PunRPC]
        void SpawnCardOnTable(string cardName)
        {
            GameObject newCard = PhotonNetwork.Instantiate(cardPrefab.name, tablePosition.position, Quaternion.identity);
            newCard.name = cardName; // Assigne le même nom que la carte ramassée
            newCard.name = cardData.name; // Assigne le même nom que la carte ramassée
            newCard.GetComponent<Renderer>().material = cardData.material; // Applique la bonne texture
        }

        

        void SpawnCardOnTable(CardData cardData)
        {
            Debug.Log("SpawnCardOnTable called");
            GameObject newCard = Instantiate(cardPrefab, tablePosition.position, Quaternion.identity);
            newCard.name = cardData.name; // Assigne le même nom que la carte ramassée
            newCard.GetComponent<Renderer>().material = cardData.material; // Applique la bonne texture
        }
    }

}
