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

    public CardDealer cardDealer;

    public BotController bot;  // Référence au bot

    public List<PlayerController> players; // Liste des joueurs humains

    private Dictionary<int, float> playerBets = new Dictionary<int, float>();

    private int currentPlayerIndex = 0; // Index du joueur courant

    //private bool allPlayersHaveBet = false; // Indicateur pour vérifier si tous les joueurs ont misé

    // UI Elements

    public TMP_Text betMessageText; // Affichage du message de mise

    public TMP_Text winnerText; // Affichage du gagnant


    // Démarre la partie et distribue les cartes
    /*
   void Start()
    {
        // Auto-assignation si les références ne sont pas remplies
        if (cardDealer == null)
        cardDealer = FindObjectOfType<CardDealer>();

    if (bot == null)
        bot = FindObjectOfType<BotController>();

        if (betMessageText == null)
        {
            GameObject betObj = GameObject.Find("Message");
            if (betObj != null) betMessageText = betObj.GetComponent<TMP_Text>();
        }

        if (winnerText == null)
        {
            GameObject winnerObj = GameObject.Find("Message");
            if (winnerObj != null) winnerText = winnerObj.GetComponent<TMP_Text>();
        }
    }*/


    public void StartRound(List<PlayerController> playerControllers)

    {

        players = playerControllers;

        currentPlayerIndex = 0;

        //allPlayersHaveBet = false;

        NextTurn();

    }


    // Gestion du tour suivant

    void NextTurn()

    {

        if (currentPlayerIndex >= players.Count) 

        {

            // Tour du bot
            Debug.Log($"pour le bot {bot== null}" );
            bot.PlayTurn();

        }

        else

        {

            // Tour du joueur humain
            Debug.Log("pour les joueurs");

            players[currentPlayerIndex].EnablePlayerTurn();

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
        if (photonView != null)
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


        //allPlayersHaveBet = true;

        RevealDealerCards();

    }


    // Révéler les cartes du dealer

    void RevealDealerCards()

    {

        photonView.RPC("DealCommunityCards", RpcTarget.AllBuffered);

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


        EndGame();

    }


    // Terminer la partie

    void EndGame()

    {

        Debug.Log("Game over, no new round.");

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