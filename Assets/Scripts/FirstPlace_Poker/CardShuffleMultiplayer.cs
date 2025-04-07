using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public class CardShuffleMultiplayer : MonoBehaviourPunCallbacks
{
    private List<Transform> deck; 
    public Transform leftPile, rightPile, mergePosition; 
    public float shuffleSpeed = 0.2f; 
    public Transform[] playerPositions; // Positions des cartes des joueurs
    private Dictionary<int, List<Transform>> playerHands = new Dictionary<int, List<Transform>>();

    void Start()
    {
        if (deck == null) deck = new List<Transform>();
    
        foreach (Transform card in transform) // Récupère tous les enfants
        {
            deck.Add(card);
        }
    }

    public void StartShuffling()
    {
        if (PhotonNetwork.IsMasterClient) // Seul le Master Client mélange
        {
            ShuffleDeck();
        }
    }
    public void ShuffleDeck()
    {
        List<int> shuffledIndices = new List<int>();
        for (int i = 0; i < deck.Count; i++) shuffledIndices.Add(i);
        ShuffleList(shuffledIndices);
        photonView.RPC("SyncShuffle", RpcTarget.All, shuffledIndices.ToArray());
    }

    IEnumerator DealCards()
{
    Debug.Log("🃏 Distribution des cartes...");

    Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

    for (int i = 0; i < players.Length; i++)
    {
        int playerID = players[i].ActorNumber;
        playerHands[playerID] = new List<Transform>();

        for (int j = 0; j < 2; j++) // 2 cartes par joueur
        {
            if (deck.Count == 0) break;

            Transform card = deck[0];
            deck.RemoveAt(0);

            playerHands[playerID].Add(card);
            photonView.RPC("GiveCardToPlayer", RpcTarget.AllBuffered, card.GetComponent<PhotonView>().ViewID, playerID, j);
        }
    }
    yield return null;
}

    [PunRPC]
    void SyncShuffle(int[] shuffledIndices)
    {
        StartCoroutine(RiffleShuffle(shuffledIndices));
    }

    IEnumerator RiffleShuffle(int[] shuffledIndices)
    {
        List<Transform> leftHalf = new List<Transform>();
        List<Transform> rightHalf = new List<Transform>();

        for (int i = 0; i < shuffledIndices.Length; i++)
        {
            if (i < shuffledIndices.Length / 2)
                leftHalf.Add(deck[shuffledIndices[i]]);
            else
                rightHalf.Add(deck[shuffledIndices[i]]);
        }

        // Déplace les cartes en deux piles
        for (int i = 0; i < leftHalf.Count; i++)
        {
            leftHalf[i].DOMove(leftPile.position + new Vector3(0, i * 0.01f, 0), shuffleSpeed);
            rightHalf[i].DOMove(rightPile.position + new Vector3(0, i * 0.01f, 0), shuffleSpeed);
        }

        yield return new WaitForSeconds(shuffleSpeed + 0.2f);

       for (int i = 0; i < Mathf.Max(leftHalf.Count, rightHalf.Count); i++)
        {
            if (i < rightHalf.Count) 
            {
                rightHalf[i].DOMove(mergePosition.position + new Vector3(0, i * 0.001f, 0), shuffleSpeed);
                yield return new WaitForSeconds(shuffleSpeed / 2);
            }

            if (i < leftHalf.Count) 
            {
                leftHalf[i].DOMove(mergePosition.position + new Vector3(0, i * 0.001f, 0), shuffleSpeed);
                yield return new WaitForSeconds(shuffleSpeed / 2);
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    void ShuffleList(List<int> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    private void GiveCardToPlayer(int cardViewID, int playerID, int cardIndex)
{
    PhotonView cardPV = PhotonView.Find(cardViewID);
    if (cardPV == null)
    {
        Debug.LogError($"❌ ERREUR : Carte avec ViewID {cardViewID} introuvable !");
        return;
    }

    GameObject card = cardPV.gameObject;

    if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
    {
        Transform playerCardPosition = playerPositions[PhotonNetwork.LocalPlayer.ActorNumber - 1];
        card.transform.SetParent(playerCardPosition);
        card.transform.localPosition = new Vector3(cardIndex * 0.5f, 0, 0);
        card.transform.localRotation = Quaternion.identity;
    }
}
}
