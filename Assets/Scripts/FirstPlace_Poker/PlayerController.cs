using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UI;


using System.Collections;
using Photon.Pun;
using DG.Tweening;
using InteractionScripts;
using Photon.Realtime;


public class PlayerController : MonoBehaviour

{

    public int playerID;

    public PokerGameManager pokerGameManager;

    public List<Card> hand = new List<Card>();

    private bool isMyTurn = false;

    private bool hasBet = false;



    // UI Elements

    public GameObject betUI;

    public GameObject pauseObj;

    public TMP_InputField betInputField;

    public Button submitBetButton;

    public TMP_Text errorBetText;


    void Start()

    {

        if (betUI != null)

        {

            betUI.SetActive(false);

        }


        if (submitBetButton != null)

        {

            submitBetButton.onClick.AddListener(OnSubmitBetClicked);

        }

        
         //StartCoroutine(RegisterToGameManager());
          StartCoroutine(WaitForPokerManager());

    }

    
IEnumerator WaitForPokerManager()
{
    while (PokerGameManager.Instance == null)
        yield return null;

    pokerGameManager = PokerGameManager.Instance;
}



    public void EnablePlayerTurn()

    {

        isMyTurn = true;

        hasBet = false;

        Debug.Log("c'est ok ici aussi");
        Debug.Log(betUI != null);
        PlayerScript pm = GetComponent<PlayerScript>();
        betUI.SetActive(true);
        //pm.showPanelOnCursor = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

public void EnableBetButton()
{
    submitBetButton.interactable = true;
}
void OnSubmitBetClicked()
{
    if (!isMyTurn) return;

    float betAmount;

    // Vérifie si le texte peut être converti en float
    if (!float.TryParse(betInputField.text, out betAmount))
    {
        errorBetText.text = "Incorrect value. Try again!";
        return;
    }

    // Vérifie si la mise est suffisante
    if (betAmount < 50)
    {
        errorBetText.text = $"Your bet is {betAmount}, but you need at least 50.";
        return;
    }

    // Mise valide : on continue
    errorBetText.text = "";
    betUI.SetActive(false);
    isMyTurn = false;
    hasBet = true;

    // 🔽 Verrouille le curseur **seulement ici**
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    PlayerScript pm = GetComponent<PlayerScript>();
    //pm.showPanelOnCursor = true;

    Debug.Log($"{playerID}");
    Debug.Log("Le Lilllllllllllllllllllllllllllllllllllllllllllllly player ID est : " + playerID);

    if (pokerGameManager == null)
    {
        Debug.LogError("[PlayerController] pokerGameManager est NULL au moment de parier !");
        return;
    }

    pokerGameManager.PlayerHasBet(playerID, betAmount);
}



    public bool HasPlacedBet()

    {

        return hasBet;

    }

    public void SetHandCards(Card card)
    {
        hand.Add(card);
    }

}
