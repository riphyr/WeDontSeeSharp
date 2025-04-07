using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;



public enum HandRank

{

    HighCard = 1,

    OnePair,

    TwoPair,

    ThreeOfAKind,

    Straight,

    Flush,

    FullHouse,

    FourOfAKind,

    StraightFlush,

    RoyalFlush

}



public class EvaluatedHand : IComparable<EvaluatedHand>

{

    public HandRank Rank;

    public List<int> HighCards; // pour départager les égalités



    public int CompareTo(EvaluatedHand other)

    {

        if (Rank != other.Rank)

            return Rank.CompareTo(other.Rank);

        for (int i = 0; i < HighCards.Count; i++)

        {

            if (i >= other.HighCards.Count) return 1;

            if (HighCards[i] != other.HighCards[i])

                return HighCards[i].CompareTo(other.HighCards[i]);

        }

        return 0;

    }

}



public class HandEvaluator : MonoBehaviour

{

    public static EvaluatedHand EvaluateHand(List<CardData> cards)

    {

        if (cards.Count != 7)

            throw new ArgumentException("You must provide exactly 7 cards.");



        var allCombos = GetAllFiveCardCombos(cards);

        EvaluatedHand bestHand = null;



        foreach (var combo in allCombos)

        {

            var evaluated = EvaluateFiveCardHand(combo);

            if (bestHand == null || evaluated.CompareTo(bestHand) > 0)

            {

                bestHand = evaluated;

            }

        }



        return bestHand;

    }



    private static List<List<CardData>> GetAllFiveCardCombos(List<CardData> cards)

    {

        var results = new List<List<CardData>>();

        var n = cards.Count;

        for (int i = 0; i < n - 4; i++)

            for (int j = i + 1; j < n - 3; j++)

                for (int k = j + 1; k < n - 2; k++)

                    for (int l = k + 1; l < n - 1; l++)

                        for (int m = l + 1; m < n; m++)

                        {

                            results.Add(new List<CardData> { cards[i], cards[j], cards[k], cards[l], cards[m] });

                        }

        return results;

    }



    private static EvaluatedHand EvaluateFiveCardHand(List<CardData> cards)

    {

        var values = cards.Select(c => (int)c.rank).OrderByDescending(v => v).ToList();

        var suits = cards.GroupBy(c => c.suit);

        var valuesGrouped = cards.GroupBy(c => (int)c.rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).ToList();



        bool isFlush = suits.Any(g => g.Count() >= 5);

        bool isStraight = IsStraight(values, out List<int> straightHighs);

        bool isStraightFlush = false;



        if (isFlush)

        {

            var flushSuit = suits.First(g => g.Count() >= 5).Key;

            var flushCards = cards.Where(c => c.suit == flushSuit).Select(c => (int)c.rank).Distinct().OrderByDescending(v => v).ToList();

            isStraightFlush = IsStraight(flushCards, out List<int> sfHighs);



            if (isStraightFlush)

            {

                if (sfHighs[0] == 14 && sfHighs[1] == 13) // A-K-Q-J-10

                    return new EvaluatedHand { Rank = HandRank.RoyalFlush, HighCards = sfHighs };

                return new EvaluatedHand { Rank = HandRank.StraightFlush, HighCards = sfHighs };

            }

        }



        if (valuesGrouped[0].Count() == 4)

        {

            var kicker = valuesGrouped[1].Key;

            return new EvaluatedHand { Rank = HandRank.FourOfAKind, HighCards = new List<int> { valuesGrouped[0].Key, kicker } };

        }



        if (valuesGrouped[0].Count() == 3 && valuesGrouped[1].Count() >= 2)

        {

            return new EvaluatedHand { Rank = HandRank.FullHouse, HighCards = new List<int> { valuesGrouped[0].Key, valuesGrouped[1].Key } };

        }



        if (isFlush)

        {

            var flushHighCards = cards.Where(c => c.suit == suits.First(g => g.Count() >= 5).Key)

                                      .Select(c => (int)c.rank).OrderByDescending(v => v).Take(5).ToList();

            return new EvaluatedHand { Rank = HandRank.Flush, HighCards = flushHighCards };

        }



        if (isStraight)

        {

            return new EvaluatedHand { Rank = HandRank.Straight, HighCards = straightHighs };

        }



        if (valuesGrouped[0].Count() == 3)

        {

            var kickers = valuesGrouped.Skip(1).Select(g => g.Key).Take(2).ToList();

            return new EvaluatedHand { Rank = HandRank.ThreeOfAKind, HighCards = new List<int> { valuesGrouped[0].Key }.Concat(kickers).ToList() };

        }



        if (valuesGrouped[0].Count() == 2 && valuesGrouped[1].Count() == 2)

        {

            var kicker = valuesGrouped.Skip(2).FirstOrDefault()?.Key ?? 0;

            return new EvaluatedHand { Rank = HandRank.TwoPair, HighCards = new List<int> { valuesGrouped[0].Key, valuesGrouped[1].Key, kicker } };

        }



        if (valuesGrouped[0].Count() == 2)

        {

            var kickers = valuesGrouped.Skip(1).Select(g => g.Key).Take(3).ToList();

            return new EvaluatedHand { Rank = HandRank.OnePair, HighCards = new List<int> { valuesGrouped[0].Key }.Concat(kickers).ToList() };

        }



        return new EvaluatedHand { Rank = HandRank.HighCard, HighCards = values.Take(5).ToList() };

    }



    private static bool IsStraight(List<int> values, out List<int> straightHighs)

    {

        var distinct = values.Distinct().OrderByDescending(v => v).ToList();

        if (distinct.Contains(14)) // check for wheel: A-2-3-4-5

            distinct.Add(1);



        for (int i = 0; i <= distinct.Count - 5; i++)

        {

            if (distinct[i] - 1 == distinct[i + 1] &&

                distinct[i + 1] - 1 == distinct[i + 2] &&

                distinct[i + 2] - 1 == distinct[i + 3] &&

                distinct[i + 3] - 1 == distinct[i + 4])

            {

                straightHighs = new List<int> { distinct[i], distinct[i + 1], distinct[i + 2], distinct[i + 3], distinct[i + 4] };

                return true;

            }

        }



        straightHighs = null;

        return false;

    }



	public static int GetWinner(List<List<CardData>> playersHands, List<CardData> boardCards)

{

    int winnerIndex = -1;

    EvaluatedHand bestHand = null;



    for (int i = 0; i < playersHands.Count; i++)

    {

        var fullHand = new List<CardData>();

        fullHand.AddRange(playersHands[i]);

        fullHand.AddRange(boardCards);



        var evaluated = EvaluateHand(fullHand);



        if (bestHand == null || evaluated.CompareTo(bestHand) > 0)

        {

            bestHand = evaluated;

            winnerIndex = i;

        }

        else if (evaluated.CompareTo(bestHand) == 0)

        {

            // Égalité, tu peux gérer un split ici si tu veux

            // Par défaut on garde le premier qui a cette main

        }



        //Debug.Log($"Joueur {i + 1} => {evaluated.Rank} - HighCards: {string.Join(",", evaluated.HighCards)}");

    }



    //Debug.Log($"Gagnant : Joueur {winnerIndex + 1} avec {bestHand.Rank}");

    return winnerIndex;

}



}

