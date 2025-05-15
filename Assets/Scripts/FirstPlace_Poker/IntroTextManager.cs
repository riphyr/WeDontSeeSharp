using UnityEngine;
using TMPro;
using System.Collections;

namespace Consigne{
public class IntroTextUI : MonoBehaviour
{
    public static IntroTextUI Instance;
    public TextMeshProUGUI introText;
    public float displayTime = 5f;

    private void Awake()
    {
        Instance = this;
        introText.gameObject.SetActive(false);
    }

    public void ShowText(string message)
    {
        introText.text = message;
        introText.gameObject.SetActive(true);
        StartCoroutine(HideTextAfterDelay());
    }

    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        introText.gameObject.SetActive(false);
    }
}
}
