using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using Photon.Pun;
using TMPro;


namespace InteractionScripts
{
    public class ChairInteraction : MonoBehaviourPunCallbacks
    {
        [Header("Chaise - Joueur")]
        public Transform sittingPosition;
        private bool isOccupied = false;
        private GameObject playerSeated = null;
        public TextMeshProUGUI infoText; 
        public bool IsOccupied => isOccupied;
        public GameObject PlayerSeated => playerSeated;
        private static int playersSeatedCount = 0; // Compteur de joueurs assis
        private static int totalPlayers = 0; // Nombre total de joueurs
        public int SeatedPlayerID { get; private set; } = -1;


        public bool CanSit()
        {
            return !isOccupied;
        }

        public void SitDown(GameObject player)
        {   
            if (!isOccupied && playerSeated == null)
            {
                PhotonView pv = player.GetComponent<PhotonView>();
                if (pv == null)
                {
                    return;
                }
                photonView.RPC("SitDownRPC", RpcTarget.AllBuffered, pv.ViewID);
            }
        }


      [PunRPC]
      /*void SitDownRPC(int playerViewID)
      {
          PhotonView playerPhotonView = PhotonView.Find(playerViewID);
          if (playerPhotonView == null) return;

          GameObject player = playerPhotonView.gameObject;
          SeatedPlayerID = playerPhotonView.Owner.ActorNumber;

          isOccupied = true;
          playerSeated = player;
          player.transform.position = sittingPosition.position;
          player.transform.rotation = sittingPosition.rotation;

          var playerController = player.GetComponent<PlayerScript>(); 
          if (playerController != null)
          {
              playerController.SitDown(sittingPosition);
          }

          playersSeatedCount++;

          if (playersSeatedCount >= totalPlayers)
          {
              photonView.RPC("StartGameSequence", RpcTarget.AllBuffered);
          }
      }*/

void SitDownRPC(int playerViewID)
{
    PhotonView playerPhotonView = PhotonView.Find(playerViewID);
    if (playerPhotonView == null) return;
    SeatedPlayerID = playerPhotonView.Owner.ActorNumber;

    GameObject player = playerPhotonView.gameObject;
    isOccupied = true;
    playerSeated = player;
    player.transform.position = sittingPosition.position;
    player.transform.rotation = sittingPosition.rotation;
    //player.GetComponent<Rigidbody>().isKinematic = true;
    var playerController = player.GetComponent<PlayerScript>(); 
    if (playerController != null)
    {
        playerController.SitDown();
    }

    Animator animator = player.GetComponentInChildren<Animator>();
    if (animator != null)
    {
        animator.SetTrigger("SitDown");
    }

    playersSeatedCount++; // Incrémente le compteur de joueurs assis

    if (playersSeatedCount >= totalPlayers) 
    {
        Debug.Log("🎭 Tous les joueurs sont assis !");
        photonView.RPC("StartGameSequence", RpcTarget.AllBuffered);
    }
}

[PunRPC]

void StartGameSequence()
        {
            StartCoroutine(StartShuffleAfterDelay());
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            totalPlayers = PhotonNetwork.PlayerList.Length;
            Debug.Log("📌 Mise à jour des joueurs dans la partie : " + totalPlayers);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            totalPlayers = PhotonNetwork.PlayerList.Length;
            Debug.Log("📌 Un joueur est parti. Joueurs restants : " + totalPlayers);
        }

        private IEnumerator DealCardsAfterDelay()
        {
            yield return new WaitForSeconds(1f); // Attendre un moment (par exemple, 1 seconde)

            CardDealer cardDealer = FindObjectOfType<CardDealer>();
            if (cardDealer != null)
            {
                cardDealer.DealCards(); // Distribue les cartes
            }
            else
            {
                Debug.LogError("❌ ERREUR : Aucun script de gestion des cartes trouvé !");
            }
        }

        private IEnumerator StartShuffleAfterDelay()
        {
            if (infoText != null)
            {
                infoText.gameObject.SetActive(true);
                Debug.Log("✅ Texte activé !");

                TypewriterEffect typewriter = infoText.GetComponent<TypewriterEffect>();
                if (typewriter != null)
                {
                    Debug.Log("✍️ Lancement de l'effet d'écriture...");
                    typewriter.StartTyping();
                }
                else
                {
                    Debug.LogError("❌ ERREUR : Pas de TypewriterEffect sur infoText !");
                }
            }
            else
            {
                Debug.LogError("❌ ERREUR : infoText est NULL !");
            }

            yield return new WaitForSeconds(4f);

            Debug.Log("🎴 Mélange des cartes !");
            CardDealer cardShuffle = FindObjectOfType<CardDealer>();
            if (cardShuffle != null)
            {
                cardShuffle.StartShuffling();
            }
            else
            {
                Debug.LogError("❌ ERREUR : Aucun script de mélange de cartes trouvé !");
            }

            // Appel pour distribuer les cartes après le mélange
            StartCoroutine(DealCardsAfterDelay());
        }
    }

/*
void StartGameSequence()
{
    StartCoroutine(StartShuffleAfterDelay());
}

public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
{
    totalPlayers = PhotonNetwork.PlayerList.Length;
    Debug.Log("📌 Mise à jour des joueurs dans la partie : " + totalPlayers);
}

public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
{
    totalPlayers = PhotonNetwork.PlayerList.Length;
    Debug.Log("📌 Un joueur est parti. Joueurs restants : " + totalPlayers);
}

private IEnumerator DealCardsAfterDelay()
{
    yield return new WaitForSeconds(1f); // Attendre un moment (par exemple, 1 seconde)

    CardDealer cardDealer = FindObjectOfType<CardDealer>();
    if (cardDealer != null)
    {
        cardDealer.DealCards(); // Distribue les cartes
    }
    else
    {
        Debug.LogError("❌ ERREUR : Aucun script de gestion des cartes trouvé !");
    }
}

   private IEnumerator StartShuffleAfterDelay()
{
    if (infoText != null)
    {
        infoText.gameObject.SetActive(true);
        Debug.Log("✅ Texte activé !");

        TypewriterEffect typewriter = infoText.GetComponent<TypewriterEffect>();
        if (typewriter != null)
        {
            Debug.Log("✍️ Lancement de l'effet d'écriture...");
            typewriter.StartTyping();
        }
        else
        {
            Debug.LogError("❌ ERREUR : Pas de TypewriterEffect sur infoText !");
        }
    }
    else
    {
        Debug.LogError("❌ ERREUR : infoText est NULL !");
    }

    yield return new WaitForSeconds(4f);

    Debug.Log("🎴 Mélange des cartes !");
    CardShuffleMultiplayer cardShuffle = FindObjectOfType<CardShuffleMultiplayer>();
    if (cardShuffle != null)
    {
        cardShuffle.StartShuffling();
    }
    else
    {
        Debug.LogError("❌ ERREUR : Aucun script de mélange de cartes trouvé !");
    }
}

}*/
}

