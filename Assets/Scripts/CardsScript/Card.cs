using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// SCRIPT PRESENT SUR LES CARTES !!!!!

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    public class Card : MonoBehaviourPunCallbacks
    {
        public string cardName; // Nom de la carte

        void Start()
        {
            cardName = name;
        }
        public void Collect()
        {
            if (photonView == null)
            {
                return;
            }

            // Ajouter la carte à l’inventaire
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(DestroyCard), RpcTarget.AllBuffered);
            }
            else
            {
                photonView.RPC(nameof(RequestDestroyCard), RpcTarget.MasterClient);
            }

            CardManager.instance.CollectCard(cardName);
        }



        [PunRPC]
        void RequestDestroyCard()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(DestroyCard), RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        void DestroyCard()
        {
            gameObject.SetActive(false); 
            Destroy(gameObject, 0.2f); 
        }
    }
}