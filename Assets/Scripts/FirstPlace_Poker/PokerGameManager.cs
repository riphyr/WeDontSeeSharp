using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Unity.VisualScripting;


public class PokerGameManager : MonoBehaviourPun

{
    private static readonly int StandUp = Animator.StringToHash("StandUp");

    public static PokerGameManager Instance { get; private set; }
    //private PhotonView photonView1;
    public CardDealer cardDealer;

    public BotController bot;  // Référence au bot

    public List<PlayerController> players; // Liste des joueurs humains

    private Dictionary<int, float> playerBets = new Dictionary<int, float>();

    private int currentPlayerIndex = 0; // Index du joueur courant

    private bool allPlayersHaveBet = false; // Indicateur pour vérifier si tous les joueurs ont misé
    
    public TMP_Text gameStartText; // Texte pour afficher "Here the game can really start"
    public Transform hubSpawnPoint; // Position dans le hub où les joueurs vont être téléportés


    // UI Elements

    public TMP_Text betMessageText; // Affichage du message de mise

    public TMP_Text winnerText; // Affichage du gagnant
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void StartRound(List<PlayerController> playerControllers)

    {

        players = playerControllers;

        currentPlayerIndex = 0;

        allPlayersHaveBet = false;

        NextTurn();

    }


    // Gestion du tour suivant

    void NextTurn()

    {

        if (currentPlayerIndex >= players.Count && !allPlayersHaveBet) 

        {
            Debug.Log("t'es appele en boucle c'est ca ???");
            // Tour du bot
            Debug.Log($"pour le bot {bot== null}" );
            bot.PlayTurn();
            allPlayersHaveBet = true;
        }

        else if(!allPlayersHaveBet)
        {

            // Tour du joueur humain
            Debug.Log("pour les joueurs");

            players[currentPlayerIndex].EnablePlayerTurn();

        }
        else{
            CheckIfAllPlayersHaveBet();
        }
        

    }




    // Mise d'un joueur

    public void PlayerHasBet(int playerId, float betAmount)

    {



        if (!playerBets.ContainsKey(playerId))

        {

            playerBets.Add(playerId, betAmount);

        }

        else

        {

            playerBets[playerId] = betAmount;

        }

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true); // Active l'objet temporairement
        }
        currentPlayerIndex++;
        Debug.Log($"{playerBets[playerId]} les bets des gens");
        Debug.Log("PhontonView de l'objet LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLily = "+ photonView.ViewID);
        //if (photonView != null)
        if (photonView != null && photonView.ViewID != 0)
        {
            // Ajout d'un petit délai pour garantir la synchronisation
            photonView.RPC("NotifyOtherPlayersOfBet", RpcTarget.All, playerId, betAmount);
        }
        else
        {
            Debug.LogError("PhotonView is null on PokerGameManager!");
        }
        
        NextTurn();

    }



    // Vérifie si tous les joueurs ont misé

    void CheckIfAllPlayersHaveBet()

    {

        foreach (var player in players)

        {

            if (!player.HasPlacedBet())

            {

                return;

            }

        }


        allPlayersHaveBet = true;

        RevealDealerCards();
        StartCoroutine(WaitBeforeWinning());

    }
    private IEnumerator WaitBeforeWinning()
{
    yield return new WaitForSeconds(3f); // Attendre 1 seconde (ajuste si besoin)

   DetermineWinner(players);
}


    // Révéler les cartes du dealer

    void RevealDealerCards()

    {
        
        cardDealer.GetComponent<PhotonView>().RPC("DealCommunityCards", RpcTarget.AllBuffered);

        Debug.Log("Les cartes du dealer sont révélées !");

    }


    // Déterminer le gagnant

    public void DetermineWinner(List<PlayerController> players)

    {

        List<EvaluatedHand> playerHands = new List<EvaluatedHand>();

        foreach (var player in players)

        {
            List<CardData> fullHand = new List<CardData>();
            fullHand.AddRange(player.hand.Select(c => c.ToCardData()));
            fullHand.AddRange(cardDealer.communityCards.Select(c => c.ToCardData()));
            playerHands.Add(HandEvaluator.EvaluateHand(fullHand));  // Evaluates hands using HandEvaluator

        }
        List<CardData> fullHandi = new List<CardData>();
        fullHandi.AddRange(bot.hand.Select(c => c.ToCardData()));
        fullHandi.AddRange(cardDealer.communityCards.Select(c => c.ToCardData()));
        playerHands.Add(HandEvaluator.EvaluateHand(fullHandi));

        EvaluatedHand bestHand = playerHands.OrderByDescending(hand => hand).FirstOrDefault();

        var winners = players.Where((player, index) => playerHands[index].CompareTo(bestHand) == 0).ToList();


        if (winners.Count == 1)

        {

            winnerText.text = $"Player {winners[0].playerID} wins!";

        }

        else

        {

            winnerText.text = "It's a tie!";

        }

        StartCoroutine(DelayedEndGame());

        //EndGame();

    }
    private IEnumerator DelayedEndGame()
    {
        yield return new WaitForSeconds(3f); // Attendre 3 secondes
        EndGame();
    }


    // Terminer la partie

    public void EndGame()
    {
        Debug.Log("Game over, no new round.");

        // Affiche le message "Here the game can really start"
        if (gameStartText != null)
        {
            gameStartText.text = "Here the game can really start!";
        }

        // Lance la coroutine de téléportation
        StartCoroutine(TeleportPlayersAfterDelay(5f)); // 5 secondes de délai
    }
    
    private IEnumerator TeleportPlayersAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player != null)
            {
                Debug.Log("HERE WE AREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
                // On récupère le PhotonView attaché au joueur
                GameObject playerObject = player.TagObject as GameObject;
                if (playerObject != null)
                {
                    GameObject enfantTrouve = playerObject.GetComponentsInChildren<Transform>(true)
                        .FirstOrDefault(t => t.name == "Player")?.gameObject;
                    
                    if (enfantTrouve != null)
                    {
                        Debug.Log(enfantTrouve.name);
                        PhotonView pv = enfantTrouve.GetComponent<PhotonView>();
                        if (pv != null)
                        {
                            gameStartText.text = "";
                            // Appel de la RPC pour tous les joueurs
                            photonView.RPC("StandUpRPC", RpcTarget.AllBuffered, pv.ViewID, hubSpawnPoint.position);
                        }
                    }
                }
            }
        }
    }
    
    [PunRPC]
    public void StandUpRPC(int playerViewID, Vector3 teleportPosition)
    {
        PhotonView playerPhotonView = PhotonView.Find(playerViewID);
        if (playerPhotonView == null) return;

        GameObject player = playerPhotonView.gameObject;

        var controller = player.GetComponent<PlayerScript>(); // adapte au nom réel de ton script
        if (controller != null)
        {
            controller.StandUp();
        }
        
        player.transform.position = teleportPosition;
        
    }

    // Affichage de la mise des autres joueurs

    [PunRPC]

    void NotifyOtherPlayersOfBet(int playerId, float betAmount)

    {

        betMessageText.text = $"Player {playerId} placed a bet of {betAmount}.";

        StartCoroutine(ClearBetMessage());

    }


    IEnumerator ClearBetMessage()

    {

        yield return new WaitForSeconds(3f);

        betMessageText.text = "";

    }

}