using TMPro;
using UnityEngine;

public class CardCounterUI : MonoBehaviour
{
    public TextMeshProUGUI cardCounterText;
    
    void Update()
    {
        if (CardsManager.instance != null)
        {
            int count = CardsManager.instance.GetPlayerCardCount();
            int total =CardsManager.instance.totalCardsToCollect;
            cardCounterText.text = $"Cards collected : {count} / {total}";
        }
    }
}