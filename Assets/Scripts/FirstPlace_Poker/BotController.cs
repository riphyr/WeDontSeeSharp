using System.Collections;

using UnityEngine;

using Photon.Pun;
using System.Collections.Generic;


public class BotController : MonoBehaviourPun

{

    public int botID = -1;

    public PokerGameManager gameManager;

    private bool hasPlacedBet = false;

    private const float botBetAmount = 50f;
     public List<Card> hand = new List<Card>();


    public void SetBotCards(List<Card> cards)

    {

        hand = cards;

    }


    void Start()

    {

        if (!PhotonNetwork.IsMasterClient)

        {

            enabled = false;  // Seul le MasterClient contrôle le bot

        }
    

    }


    public void PlayTurn()

    {
        Debug.Log("donc non ?");
        StartCoroutine(DecideAction());

    }


    private IEnumerator DecideAction()

    {

        yield return new WaitForSeconds(Random.Range(1f, 3f));


        PlaceBet();


        // Passer le tour après avoir misé

        gameManager.PlayerHasBet(botID, botBetAmount);

    }


    void PlaceBet()

    {

        hasPlacedBet = true;

        Debug.Log($"Bot {botID} made a bet of {botBetAmount}");

    }


    public bool HasPlacedBet()

    {

        return hasPlacedBet;

    }

}


