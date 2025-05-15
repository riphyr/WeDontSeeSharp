using System.Collections;

using UnityEngine;

using Photon.Pun;
using System.Collections.Generic;


public class BotController : MonoBehaviourPun

{

    public int botID = 555;

    public PokerGameManager pokerGameManager;

    private bool hasPlacedBet = false;

    private const float botBetAmount = 50f;
     public List<Card> hand = new List<Card>();


    void Start()
    {
         StartCoroutine(WaitForPokerManager());
    }

    public void SetBotCards(List<Card> cards)

    {

        hand = cards;

    }





IEnumerator WaitForPokerManager()
{
    while (PokerGameManager.Instance == null)
        yield return null;

    pokerGameManager = PokerGameManager.Instance;
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

        Debug.Log($"{pokerGameManager == null} VRAIMENTTTTTTTTTTTTTTTTTTTTT?????????Q");
        // Passer le tour après avoir misé

        pokerGameManager.PlayerHasBet(botID, botBetAmount);

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


