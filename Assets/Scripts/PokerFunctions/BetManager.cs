using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetManager : MonoBehaviour
{
    public TMP_InputField betInputField;
    public TextMeshProUGUI currentPlayerText;
    public Button submitButton;
    public int numberOfPlayers = 5;

    private int[] betAmounts;
    private int currentPlayer = 0;

    void UpdateCurrentPlayerText()
    {
        currentPlayerText.text = $"Player: {currentPlayer + 1}";
    }

    // Start is called before the first frame update
    void SubmitBet()
    {
        int betAmount;
        if (int.TryParse(betInputField.text, out betAmount))
        {
            betAmounts[currentPlayer] = betAmount;
            currentPlayer++;
            if (currentPlayer == numberOfPlayers)
            {
                //change to next scene
            }
            else
            {
                UpdateCurrentPlayerText();
            }
        }
        else
        {
            Debug.Log("Invalid bet amount entered.");
        }
    }

    void Start()
    {
        betAmounts = new int[numberOfPlayers];
        UpdateCurrentPlayerText();
        submitButton.onClick.AddListener(SubmitBet);
    }
}
