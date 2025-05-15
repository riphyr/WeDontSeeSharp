using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    public class Card : MonoBehaviourPunCallbacks
    {
        public string cardName; // Nom de la carte
        //public static List<CardData> collectedCards = new List<CardData>();
        //public Material cardMaterial;
        //public GameObject cardPrefab; // Prefab de la carte à instancier sur la table
        //public Transform tablePosition; // Position où les cartes seront déposées
        //public List<string> cardsName = new List<string>();
        private bool isNearTable = false;

        void Start()
        {
            cardName = name;
        }
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

            CardManager.instance.CollectCard(cardName);
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
    }
}
