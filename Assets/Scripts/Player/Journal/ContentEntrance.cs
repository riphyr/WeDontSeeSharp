using UnityEngine;
using TMPro;
using System.Collections;

public class ContentEntrance : MonoBehaviour
{
    [Header("Références")]
    public EntranceAnimator linkedEntrance;
    public RectTransform contentPanel;
    
    [Header("Texte à afficher")]
    public GameObject textObject;
    public TextMeshProUGUI textComponent;
    [TextArea(2, 5)] public string contentText;
    
    [Header("Animation")]
    public float scaleDuration = 0.4f;

    private bool isScaling = false;

    private void Start()
    {
        if (contentPanel != null)
        {
            contentPanel.pivot = new Vector2(1f, 0.5f);
            ResetContent();
        }
        
        if (textObject != null)
            textObject.SetActive(false);
    }

    public void AppearWithContent()
    {
        if (linkedEntrance != null)
            linkedEntrance.SlideIn();

        if (textComponent != null)
        {
            textComponent.text = contentText;
            textObject?.SetActive(true);
        }
        
        if (!isScaling && contentPanel != null)
        {
            StartCoroutine(ScaleInFromRight());
        }
    }

    public void ResetContent()
    {
        isScaling = false;

        if (contentPanel != null)
            contentPanel.localScale = new Vector3(0f, 1f, 1f);

        if (linkedEntrance != null)
            linkedEntrance.ResetSlide();
        
        if (textObject != null)
            textObject.SetActive(false);
    }

    private IEnumerator ScaleInFromRight()
    {
        isScaling = true;

        Vector3 startScale = new Vector3(0f, 1f, 1f);
        Vector3 endScale = new Vector3(1f, 1f, 1f);

        float elapsed = 0f;

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / scaleDuration);
            contentPanel.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        contentPanel.localScale = endScale;
        isScaling = false;
    }
    
    public void SetContentText(string newText)
    {
        contentText = newText;
    }
}