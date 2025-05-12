using TMPro;
using UnityEngine;

public class CardCounterUI : MonoBehaviour
{
    public TextMeshProUGUI cardCounterText;
    
    void Update()
    {
        if (CardManager.instance != null)
        {
            int count = CardManager.instance.GetPlayerCardCount();
            int total = CardManager.instance.totalCardsToCollect;
            cardCounterText.text = $"Cards collected : {count} / {total}";
        }
    }
}