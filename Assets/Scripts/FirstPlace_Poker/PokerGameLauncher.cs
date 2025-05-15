using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PokerGameLauncher : MonoBehaviourPunCallbacks
{
    public GameObject pokerManagerPrefab;

    public CardDealer cardDealer;
    public BotController bot;
    public TMP_Text betMessageText; // Affichage du message de mise
    public TMP_Text winnerText; // Affichage du gagnant
    public TMP_Text gameStartText; // Texte pour afficher "Here the game can really start"
    public Transform hubSpawnPoint; // Position dans le hub où les joueurs vont être téléportés


    void Start()
    {
        StartCoroutine(WaitForPhotonAndInstantiateManager());
        
    }
    IEnumerator WaitForPhotonAndInstantiateManager()
{
    while (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.IsMasterClient)
    {
        Debug.Log("⏳ En attente de Photon...");
        yield return null;
    }

    Debug.Log("✅ Photon est prêt et c’est le MasterClient, on instancie le manager.");
    GameObject pokerGO = PhotonNetwork.Instantiate(pokerManagerPrefab.name, Vector3.zero, Quaternion.identity);

    StartCoroutine(WaitForManagerAndSetup());
}
     System.Collections.IEnumerator WaitForManagerAndSetup()
    {
        while (PokerGameManager.Instance == null)
            yield return null;

        PokerGameManager manager = PokerGameManager.Instance;

        manager.cardDealer = cardDealer;
        manager.bot = bot;
        manager.betMessageText = betMessageText;
        manager.winnerText = winnerText;
        manager.gameStartText = gameStartText;
        manager.hubSpawnPoint = hubSpawnPoint;

        Debug.Log("PokerGameManager correctement initialisé !");
    }

    /*System.Collections.IEnumerator WaitForManagerAndSetup(GameObject managerGO)
    {
        // Attendre que le PokerGameManager soit bien initialisé
        PokerGameManager manager = null;
        while (manager == null)
        {
            manager = managerGO.GetComponent<PokerGameManager>();
            yield return null; // attendre le prochain frame
        }

        // Une fois que le manager est trouvé, l'initialiser
        manager.cardDealer = cardDealer;
        manager.bot = bot;
        manager.betMessageText = betMessageText;
        manager.winnerText = winnerText;

        Debug.Log("PokerGameManager bien configuré !");
    }*/

    // Cette méthode est appelée lorsque le joueur entre dans la room
    /*public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        // Récupérer le PlayerController associé au joueur
        GameObject playerObject = newPlayer.TagObject as GameObject;

        if (playerObject != null)
        {
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Ajouter le joueur à la liste des joueurs dans le PokerGameManager
                pokerManager.players.Add(playerController);

                // Assigner le PokerGameManager au joueur
               // playerController.pokerGameManager = pokerManager.gameObject;

                // Démarrer la partie si tous les joueurs sont prêts
                if (pokerManager.players.Count == PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    pokerManager.StartRound(pokerManager.players); // Démarre la partie
                }
            }
        }
    }*/

    // Cette méthode est appelée lorsque le joueur quitte la room
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        // Retirer le joueur de la liste des joueurs
        if (PokerGameManager.Instance != null)
        {
            GameObject playerObject = otherPlayer.TagObject as GameObject;
            if (playerObject != null)
            {
                PlayerController playerToRemove = playerObject.GetComponent<PlayerController>();
                if (playerToRemove != null)
                {
                    PokerGameManager.Instance.players.Remove(playerToRemove);
                }
            }
        }
    }
}



