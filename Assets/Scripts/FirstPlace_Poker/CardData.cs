using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData
{
    public Card.Suit suit;
    public Card.Rank rank;

    public CardData(Card.Suit suit, Card.Rank rank)
    {
        this.suit = suit;
        this.rank = rank;
    }
}

