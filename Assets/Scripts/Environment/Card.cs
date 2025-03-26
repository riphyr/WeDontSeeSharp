using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    public class Card : MonoBehaviourPunCallbacks
    {
        public string cardName;  // Nom de la carte

        public void Collect()
        {
                // Le joueur ramasse la carte et l'ajoute à son inventaire
                GameManager.instance.CollectCard(cardName);

                // Synchronisation : demander au MasterClient de détruire la carte
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("DestroyCard", RpcTarget.AllBuffered);
                }
                else
                {
                    photonView.RPC("RequestDestroyCard", RpcTarget.MasterClient);
                }
        }

        [PunRPC]
        void RequestDestroyCard()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("DestroyCard", RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        void DestroyCard()
        {
            Destroy(gameObject);
        }
    }
}
