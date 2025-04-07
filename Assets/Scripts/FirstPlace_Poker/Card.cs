using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;
using UnityEngine.UI; 

public class Card : MonoBehaviour
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace
    }

    [Header("Card Info")]
    public Suit suit;

    public Rank rank;

    // Tu peux ajouter une méthode pour obtenir une CardData si besoin :
    public CardData ToCardData()
    {
        return new CardData(suit, rank);
    }
}

/*
public class Card : MonoBehaviour

{

    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    public enum Rank

    {

        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,

        Jack, Queen, King, Ace

    }


    public Suit suit;

    public Rank rank;


    public Card(Suit suit, Rank rank)

    {

        this.suit = suit;

        this.rank = rank;

    }
    

}*/