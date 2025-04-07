using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UI;


public class PlayerController : MonoBehaviour

{

    public int playerID;

    public GameObject pokerGameManager;

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

    }


    public void EnablePlayerTurn()

    {

        isMyTurn = true;

        hasBet = false;

        Debug.Log("c'est ok ici aussi");
        Debug.Log(betUI != null);
        PlayerScript pm = GetComponent<PlayerScript>();
        betUI.SetActive(true);
        pm.showPanelOnCursor = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }


    void OnSubmitBetClicked()

    {

        if (!isMyTurn) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        float betAmount;

        if (float.TryParse(betInputField.text, out betAmount))

        {

            if (betAmount < 50)

            {

                errorBetText.text = $"Your bet is {betAmount}, but you need at least 50.";

            }

            else

            {

                betUI.SetActive(false);

                isMyTurn = false;

                hasBet = true;

                Debug.Log($"{playerID}");

                PokerGameManager pokerManager = pokerGameManager.GetComponent<PokerGameManager>();
                if (pokerManager != null)
                {
                    pokerManager.PlayerHasBet(playerID, betAmount);
                }
else
{
    Debug.LogError("PokerGameManager reference is null!");
}


            }

        }

        else

        {

            errorBetText.text = "Incorrect value. Try again!";

        }

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
