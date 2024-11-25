using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum HandType
{
    Nothing,
    OnePair,
    FourOfAKind,
    TwoPairs,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
}

public struct HandValue
{
    public int Toto { get; set; }   //Le total que le jouer a pour savoir qui gagne en presence de plusieurs mains gagnantes
    public int PlusGrand { get; set; }  //Plus grande carte dans le cas ou il n'y a pas de main gagnante ou autre
}

class HandEvaluator : Card
{
    private int HeartSum;
    private int DiamondSum;
    private int ClubSum;
    private int SpadeSum;
    private Card[] Cards;
    private HandValue HandValues;

    public HandEvaluator(Card[] hand)
    {
        HeartSum = 0;
        DiamondSum = 0;
        ClubSum = 0;
        SpadeSum = 0;
        Cards= new Card[hand.Length];
        HandValues = new HandValue();
    }

    public HandValue HandVal //Pas trop bien compris
    {
        get { return HandValues; }
        set { HandValues = value; }
    }
    
    public Card[] card
    {
        get { return Cards; }
        set
        {
            for (int i = 0; i < value.Length; i++)
            {
                Cards[i] = value[i];
            }
        }
    }

    public HandType EvaluateHand()
    {
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
        {
            { "Two", 1 }, { "Three", 2 }, { "Four", 3 },
            { "Five", 4 }, { "Six", 5}, { "Seven", 6 }, { "Eight", 7 },
            { "Nine", 8 }, { "Ten", 9 }, { "Jack", 10 }, { "Queen", 11 }, { "King", 12 }, { "As", 13 }
        };
        getNuberofSuit();
        if(FourOfKind())
            return HandType.FourOfAKind;
        else if(FullHouse())
            return HandType.FullHouse;
        else if(Flush())
            return HandType.Flush;
        else if(Straight())
            return HandType.Straight;
        else if(ThreeOfAKind())
            return HandType.ThreeOfAKind;
        else if(TwoPair())
            return HandType.TwoPairs;
        else if(Onepair())
            return HandType.OnePair;
        HandValue.PlusGrand = ordreValeurs[Cards[4].rank]; //Probleme sur le cas ou il n'y a pour aucun joueur de cas de main gagnante, evaluer qui a la plus forte carte
        return HandType.Nothing;
    }

    private void getNuberofSuit()
    {
        foreach (var elm in Cards)
        {
            if (elm.suit == "Clubs")
                ClubSum++;
            if (elm.suit == "Diamonds")
                DiamondSum++;
            if (elm.suit == "Hearts")
                HeartSum++;
            if (elm.suit == "Spades")
                SpadeSum++;
        }
    }

    private bool FourOfKind()
    {   
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
        {
            { "Two", 1 }, { "Three", 2 }, { "Four", 3 },
            { "Five", 4 }, { "Six", 5}, { "Seven", 6 }, { "Eight", 7 },
            { "Nine", 8 }, { "Ten", 9 }, { "Jack", 10 }, { "Queen", 11 }, { "King", 12 }, { "As", 13 }
        };
        if (Cards[0].rank == Cards[1].rank && Cards[1].rank == Cards[2].rank && Cards[2].rank == Cards[3].rank)
        {
            HandValues.Toto = ordreValeurs[Cards[0].rank]*4;
            HandValues.PlusGrand = ordreValeurs[Cards[4].rank];
            return true;
        }
        else if (Cards[1].rank == Cards[2].rank && Cards[2].rank == Cards[3].rank && Cards[3].rank == Cards[4].rank)
        {
            HandValues.Toto = ordreValeurs[Cards[1].rank]*4;
            HandValues.PlusGrand = ordreValeurs[Cards[0].rank];
            return true;
        }

        return false;
    }

    private bool FullHouse()
    {   
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
        {
            { "Two", 1 }, { "Three", 2 }, { "Four", 3 },
            { "Five", 4 }, { "Six", 5}, { "Seven", 6 }, { "Eight", 7 },
            { "Nine", 8 }, { "Ten", 9 }, { "Jack", 10 }, { "Queen", 11 }, { "King", 12 }, { "As", 13 }
        };
        if (Cards[0].rank == Cards[1].rank && Cards[1].rank == Cards[2].rank && Cards[3].rank == Cards[4].rank)
        {
            HandValues.Toto = ordreValeurs[Cards[0].rank]*3 + ordreValeurs[Cards[4].rank]*2;
            return true;
        }
        else if (Cards[0].rank == Cards[1].rank && Cards[2].rank == Cards[3].rank && Cards[3].rank == Cards[4].rank)
        {
            HandValues.Toto = ordreValeurs[Cards[0].rank]*2 + ordreValeurs[Cards[4].rank]*3;
            return true;
        }
        return false;
    }

    private bool Flush()
    {   
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
        {
            { "Two", 1 }, { "Three", 2 }, { "Four", 3 },
            { "Five", 4 }, { "Six", 5}, { "Seven", 6 }, { "Eight", 7 },
            { "Nine", 8 }, { "Ten", 9 }, { "Jack", 10 }, { "Queen", 11 }, { "King", 12 }, { "As", 13 }
        };
        if (SpadeSum == 5 || HeartSum == 5 || ClubSum == 5 || DiamondSum == 5)
        {
            HandValues.Toto = ordreValeurs[Cards[4].rank];
            return true;
        }

        return false;
    }

    private bool Straight()
    {
        // DEMANDER C'EST QUOI LA CARTE LA PLUS FORTE AU POKER
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
        {
            { "As", 1 }, { "Two", 2 }, { "Three", 3 }, { "Four", 4 },
            { "Five", 5 }, { "Six", 6}, { "Seven", 7 }, { "Eight", 8 },
            { "Nine", 9 }, { "Ten", 10 }, { "Jack", 11 }, { "Queen", 12 }, { "King", 13 }, 
        };
        if ((ordreValeurs[Cards[0].rank] + 1) == ordreValeurs[Cards[1].rank] && (ordreValeurs[Cards[1].rank] + 1) == ordreValeurs[Cards[2].rank] && (ordreValeurs[Cards[2].rank] + 1) == ordreValeurs[Cards[3].rank] &&
            (ordreValeurs[Cards[3].rank] + 1) == ordreValeurs[Cards[4].rank])
        {   
            HandValues.Toto = ordreValeurs[Cards[4].rank];
            return true;
        }

        return false;
    }

    private bool ThreeOfAKind()
    {   
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
        {
            { "Two", 1 }, { "Three", 2 }, { "Four", 3 },
            { "Five", 4 }, { "Six", 5}, { "Seven", 6 }, { "Eight", 7 },
            { "Nine", 8 }, { "Ten", 9 }, { "Jack", 10 }, { "Queen", 11 }, { "King", 12 }, { "As", 13 }
        };
        if ((Cards[0].rank == Cards[1].rank && Cards[1].rank == Cards[2].rank) ||
            (Cards[1].rank == Cards[2].rank && Cards[2].rank == Cards[3].rank) ||
            (Cards[2].rank == Cards[3].rank && Cards[3].rank == Cards[4].rank))
        {
            HandValues.Toto = ordreValeurs[Cards[2].rank]*3;
            HandValues.PlusGrand = ordreValeurs[Cards[4].rank];
            return true;
        }
        return false;    
    }

    private bool TwoPair()
    {   
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
        {
            { "Two", 1 }, { "Three", 2 }, { "Four", 3 },
            { "Five", 4 }, { "Six", 5}, { "Seven", 6 }, { "Eight", 7 },
            { "Nine", 8 }, { "Ten", 9 }, { "Jack", 10 }, { "Queen", 11 }, { "King", 12 }, { "As", 13 }
        };
        if (Cards[0].rank == Cards[1].rank && Cards[2].rank == Cards[3].rank)
        {
            HandValues.Toto = (ordreValeurs[Cards[1].rank]*2) + (ordreValeurs[Cards[3].rank]*2);
            HandValues.PlusGrand =  ordreValeurs[Cards[4].rank];
            return true;
        }
        else if ((Cards[0].rank == Cards[1].rank && Cards[3].rank == Cards[4].rank))
        {
            HandValues.Toto = (ordreValeurs[Cards[1].rank]*2) + (ordreValeurs[Cards[3].rank]*2);
            HandValues.PlusGrand =  ordreValeurs[Cards[2].rank];
            return true;
        }
        else if ((Cards[1].rank == Cards[2].rank && Cards[3].rank == Cards[4].rank))
        {
            HandValues.Toto = (ordreValeurs[Cards[1].rank]*2) + (ordreValeurs[Cards[3].rank]*2);
            HandValues.PlusGrand =  ordreValeurs[Cards[1].rank];
        }

        return false;
    }

    private bool Onepair()
    {   
        Dictionary<string, int> ordreValeurs = new Dictionary<string, int>
        {
            { "Two", 1 }, { "Three", 2 }, { "Four", 3 },
            { "Five", 4 }, { "Six", 5}, { "Seven", 6 }, { "Eight", 7 },
            { "Nine", 8 }, { "Ten", 9 }, { "Jack", 10 }, { "Queen", 11 }, { "King", 12 }, { "As", 13 }
        };
        if (Cards[0].rank == Cards[1].rank)
        {
            HandValues.Toto = ordreValeurs[Cards[0].rank]*2;
            HandValues.PlusGrand =  ordreValeurs[Cards[4].rank];
            return true;
        }

        else if (Cards[1].rank == Cards[2].rank)
        {
            HandValues.Toto = ordreValeurs[Cards[1].rank]*2;
            HandValues.PlusGrand =  ordreValeurs[Cards[4].rank];
            return true;
        }

        else if (Cards[2].rank == Cards[3].rank)
        {
            HandValues.Toto = ordreValeurs[Cards[2].rank]*2;
            HandValues.PlusGrand =  ordreValeurs[Cards[4].rank]; 
            return true;
        }

        else if (Cards[3].rank == Cards[4].rank)
        {
            HandValues.Toto = ordreValeurs[Cards[3].rank]*2;
            HandValues.PlusGrand =  ordreValeurs[Cards[2].rank]; 
            return true; 
        }

        return false;
    }
}