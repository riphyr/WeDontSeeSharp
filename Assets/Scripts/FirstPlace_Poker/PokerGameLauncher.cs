using UnityEngine;
using Photon.Pun;
using TMPro;

public class PokerGameLauncher : MonoBehaviour
{
    public GameObject pokerManagerPrefab;

    public CardDealer cardDealer;
    public BotController bot;
    public TMP_Text betMessageText; // Affichage du message de mise

    public TMP_Text winnerText; // Affichage du gagnant

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Instancier PokerGameManager sur le Master Client
            GameObject pokerManagerGO = PhotonNetwork.Instantiate(pokerManagerPrefab.name, Vector3.zero, Quaternion.identity);
            pokerManagerGO.SetActive(true); // Assure-toi que l'objet est actif

            StartCoroutine(WaitForManagerAndSetup(pokerManagerGO));
        }
    }

    System.Collections.IEnumerator WaitForManagerAndSetup(GameObject managerGO)
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
        manager.betMessageText =  betMessageText;
        manager.winnerText = winnerText;
        Debug.Log("PokerGameManager bien configuré !");
    }
}



