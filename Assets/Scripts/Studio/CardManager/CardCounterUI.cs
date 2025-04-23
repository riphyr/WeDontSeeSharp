using TMPro;
using UnityEngine;

public class CardCounterUI : MonoBehaviour
{
    public TextMeshProUGUI cardCounterText;
    
    void Update()
    {
        if (GameManager.instance != null)
        {
            int count = GameManager.instance.GetPlayerCardCount();
            int total = GameManager.instance.totalCardsToCollect;
            cardCounterText.text = $"Cards collected : {count} / {total}";
        }
    }
}