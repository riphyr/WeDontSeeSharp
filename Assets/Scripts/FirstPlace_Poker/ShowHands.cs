using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShowPokerHands : MonoBehaviour
{
    public CanvasGroup imageGroup;
    public GameObject betPanel;
    //public GameObject betInput;
    public Button toggleButton;
    public TMP_Text toggleButtonText;  // Adapté pour TMP
    public float fadeDuration = 0.5f;

    private bool isVisible = false;

    public void ToggleImage()
    {
        isVisible = !isVisible;

        betPanel.SetActive(!isVisible);

        toggleButtonText.text = isVisible ? "Close" : "Show Hands";

        StopAllCoroutines();
        if (isVisible)
        {
            imageGroup.gameObject.SetActive(true);
            StartCoroutine(FadeImage(0f, 1f));
        }
        else
        {
            StartCoroutine(FadeImage(1f, 0f));
        }
    }

    private IEnumerator FadeImage(float startAlpha, float endAlpha)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            imageGroup.alpha = alpha;
            yield return null;
        }

        imageGroup.alpha = endAlpha;

        if (endAlpha == 0f)
        {
            imageGroup.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        imageGroup.alpha = 0f;
        imageGroup.gameObject.SetActive(false);
        toggleButtonText.text = "Show Hands";
    }
}
