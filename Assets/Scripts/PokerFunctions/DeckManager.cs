using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Creation d'une classe Carte
public class Card
{
    public string suit;
    public string rank;
}

public class DeckManager : MonoBehaviour
{
    private List<Card> _deck = new List<Card>();
    //Creation du paquet de carte
    public void CreateDeck()
    {
        string[] suits = { "Clubs", "Diamonds", "Hearts", "Spades" };
        string[] ranks = { "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King", "Ace" };

        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                Card newCard = new Card();
                newCard.suit = suit;
                newCard.rank = rank;
                _deck.Add(newCard);
            }
        }
    }
    //Melanger le paquet de carte
    public void ShuffleDeck()
    {
        int n = _deck.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Card temp = _deck[k];
            _deck[k] = _deck[n];
            _deck[n] = temp;
        }
    }
    //Fonction Tri
    public List<Card> Comparer(List<Card> hand)
    { 
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
    {
         { "Two", 1 }, { "Three", 2 }, { "Four", 3 },
        { "Five", 4 }, { "Six", 5}, { "Seven", 6 }, { "Eight", 7 },
        { "Nine", 8 }, { "Ten", 9 }, { "Jack", 10 }, { "Queen", 11 }, { "King", 12 }, { "As", 13 }
    };
        
        for (int i = 1; i < hand.Count; i++)
        {
            int clecle = ordreValeurs[hand[i].rank];
            int j = i - 1;

            // Déplacer les éléments plus grands que la clé d'une position vers la droite
            while (j >= 0 && ordreValeurs[hand[j].rank] > clecle)
            {
                hand[j + 1] = hand[j];
                j--;
            }

            // Insérer la clé à sa position correcte
            hand[j + 1] = hand[i];
        }
        return hand;
    }    
        
    //Main d'un joueur
    public List<Card> DealCards()
    {
            List<Card> hand = new List<Card>();
            for (int j = 0; j < 5; j++) 
            {
                hand.Add(_deck[0]);
                _deck.RemoveAt(0);
            }
            List<Card> hand2 = Comparer(hand);
        return hand2;
    }
    //Main du dealer
    public List<Card> DealCardsD(bool bet) //Revoir car il faut evaluer les cas pour le nombre de carte que le dealer doit presenter
    {
            List<Card> hand = new List<Card>();
            if(bet)
            {
                for (int j = 0; j < 5; j++) 
                {
                     hand.Add(_deck[0]);
                     _deck.RemoveAt(0);
                }
            }
            else
            {
                for (int j = 0; j < 3; j++) 
                {
                    hand.Add(_deck[0]);
                    _deck.RemoveAt(0);
                }
            }
            return Comparer(hand);
    }

    
}
