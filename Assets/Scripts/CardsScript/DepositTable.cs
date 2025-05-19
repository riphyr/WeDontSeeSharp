using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace InteractionScripts
{
    public class DepositTable : MonoBehaviourPunCallbacks
    {
        [Header("Disposition")]
        public float spacing = 0.5f;
        public int cardsPerRow = 13;
        public Transform firstCardPosition;

        private List<GameObject> displayedCards = new List<GameObject>();

        private void Awake()
        {
                SceneManager.sceneLoaded += OnSceneLoaded;

        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Redessine uniquement si on est dans le Hub
            if (scene.name == "FirstPlace_Poker2") // Remplace "Hub" par le nom exact de ta scène hub
            {
                RedrawCards();
            }
        }

        public void DepositCards()
        {
            Debug.Log("==> Dépôt des cartes");

            foreach (var card in displayedCards)
            {
                Destroy(card);
            }

            displayedCards.Clear();

            var inventory = CardManager.instance.collectedCardIDs;
            Debug.Log(inventory.Count);
            var alreadyDeposited = new HashSet<string>(CardManager.instance.persistentDepositedCards);

            int cardIndex = CardManager.instance.persistentDepositedCards.Count;

            for (int i = 0; i < inventory.Count; i++)
            {
                string cardName = inventory[i];
                if (alreadyDeposited.Contains(cardName))
                    continue;

                GameObject prefab = CardManager.instance.GetCardPrefabByName(cardName);
                Debug.Log(prefab.name);
                if (prefab != null)
                {
                    int row = cardIndex / cardsPerRow;
                    int col = cardIndex % cardsPerRow;

                    Vector3 position = firstCardPosition.position + new Vector3(col * spacing, 0, -row * spacing);
                    GameObject cardInstance = PhotonNetwork.Instantiate(prefab.name, position, Quaternion.identity);
                    displayedCards.Add(cardInstance);

                    CardManager.instance.DepositCard(cardName);
                    CardManager.instance.persistentDepositedCards.Add(cardName);

                    cardIndex++;
                }
                else
                {
                    Debug.LogWarning($"Prefab introuvable pour : {cardName}");
                }
            }

            CardManager.instance.ClearInventory();
        }

        public void RedrawCards()
        {
            Debug.Log("==> Redessiner les cartes déjà déposées");

            foreach (var card in displayedCards)
            {
                Destroy(card);
            }

            displayedCards.Clear();

            var deposited = CardManager.instance.persistentDepositedCards;

            for (int i = 0; i < deposited.Count; i++)
            {
                GameObject prefab = CardManager.instance.GetCardPrefabByName(deposited[i]);
                if (prefab != null)
                {
                    int row = i / cardsPerRow;
                    int col = i % cardsPerRow;

                    Vector3 position = firstCardPosition.position + new Vector3(col * spacing, 0, -row * spacing);
                    GameObject cardInstance = Instantiate(prefab, position, Quaternion.identity);
                    displayedCards.Add(cardInstance);
                }
            }
        }
    }
}



        /*[SerializeField] private Transform[] playerDropPoints; // assignés dans l'inspector

        private Transform GetPlayerDropPoint()
        {
            //int playerID = PhotonNetwork.LocalPlayer.ActorNumber;
            int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            if (index >= playerDropPoints.Length)
                index = playerDropPoints.Length - 1; // fallback

            return playerDropPoints[index];
        }


        public void DepositCards()
        {

            int playerID = PhotonNetwork.LocalPlayer.ActorNumber;
            var inventory = CardsManager.instance.GetInventory(playerID);

            if (inventory == null || inventory.Count == 0)
            {
                Debug.Log("Aucune carte à déposer.");
                return;
            }
            Transform dropPoint = GetPlayerDropPoint();
            Vector3 spawnPos = dropPoint.position;
            //GameObject deckprefab = Resources.Load<GameObject>("DeckCard1");
            //Vector3 spawnPos = dropPoint.position;
            foreach (string cardName in inventory)
            {
                Debug.Log($"Card {cardName} deposer.");
                GameObject prefab =  CardsManager.instance.GetCardPrefabByName(cardName);
                if (prefab != null)
                {
                    Debug.Log($"Bah c'est bien deposer {prefab.name}");
                    PhotonNetwork.Instantiate(prefab.name, spawnPos, Quaternion.identity);
                    //CardCollectionManager.Instance.AddCard(cardName);
                    spawnPos += new Vector3(spacing, 0, 0); // décaler à droite
                }
                else
                {
                    Debug.Log($"Bah c'est nul :(");
                }
            }

            // Vider l'inventaire du joueur après dépôt
            CardsManager.instance.ClearInventory(playerID);
        }*/
    