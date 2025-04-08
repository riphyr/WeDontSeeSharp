using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    public class Card : MonoBehaviourPunCallbacks
    {
        public string cardName; // Nom de la carte
        public static List<CardData> collectedCards = new List<CardData>(); 
        public Material cardMaterial;
        public GameObject cardPrefab; // Prefab de la carte à instancier sur la table
        public Transform tablePosition; // Position où les cartes seront déposées
        private bool isNearTable = false;

        public void Collect()
        {
            Debug.Log($"Collect : {cardName}");

            // Vérifier si PhotonView est bien attaché
            if (photonView == null)
            {
                Debug.LogError("PhotonView est NULL !");
                return;
            }
            Debug.Log($"PhotonView ID: {photonView.ViewID}");

            // Ajouter la carte à l’inventaire

            // Demander au MasterClient de détruire la carte
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Je suis MasterClient, je détruis la carte.");
                photonView.RPC(nameof(DestroyCard), RpcTarget.AllBuffered);
            }
            else
            {
                Debug.Log("Je ne suis pas MasterClient, je demande la suppression.");
                photonView.RPC(nameof(RequestDestroyCard), RpcTarget.MasterClient);
            }
            GameManager.instance.CollectCard(cardName);
        }

        

        [PunRPC]
        void RequestDestroyCard()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("MasterClient a reçu la requête de suppression.");
                photonView.RPC(nameof(DestroyCard), RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        void DestroyCard()
        {
            Debug.Log($"[RPC] {gameObject.name} supprimé par {PhotonNetwork.LocalPlayer.NickName}");
            gameObject.SetActive(false); // Désactivation avant suppression (évite des bugs)
            Destroy(gameObject, 0.2f); // Supprime après un léger délai
        }
        
        [System.Serializable]
        public class CardData
        {
            public string name;
            public Material material;

            public CardData(string name, Material material)
            {
                this.name = name;
                this.material = material;
            }
        }
        
        
        
        public void Deposit()
        {
            Debug.Log("Deposit called");
            if (isNearTable && collectedCards.Count > 0 && Input.GetKeyDown(KeyCode.E))
            {
                CardData cardToPlace = collectedCards[0]; // Prend la première carte de la liste
                photonView.RPC("SpawnCardOnTable", RpcTarget.AllBuffered, cardToPlace.name);
                collectedCards.RemoveAt(0); // Retire la carte de l'inventaire
            }
        }
        [PunRPC]
        void SpawnCardOnTable(string cardName)
        {
            GameObject newCard = PhotonNetwork.Instantiate(cardPrefab.name, tablePosition.position, Quaternion.identity);
            newCard.name = cardName; // Assigne le même nom que la carte ramassée
            newCard.name = name; // Assigne le même nom que la carte ramassée
        }
    }
}
