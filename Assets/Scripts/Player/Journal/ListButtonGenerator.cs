using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ListButtonGenerator : MonoBehaviour
{
    [Header("Références")]
    public PreviewManager previewManager;
    public GameObject buttonPrefab;
    public Transform contentParent;
    
    [Header("SFX")]
    public AudioSource hoverSound;

    [Header("Liste d'éléments")]
    private LoreDatabase loreDatabase;
	public ContentEntrance contentEntrance;

    
    private UIButtonHoverEffect lastSelected;

    void Start()
    {
	    loreDatabase = Resources.Load<LoreDatabase>("LoreDatabase");
        GenerateButtons();
    }
    
    private void OnEnable()
    {
	    if (loreDatabase == null)
		    loreDatabase = Resources.Load<LoreDatabase>("LoreDatabase");

	    RefreshButtons();
    }

    void GenerateButtons()
	{
    	foreach (LoreEntry entry in loreDatabase.loreEntries)
    	{
        	GameObject newButton = Instantiate(buttonPrefab, contentParent);

        	TMP_Text textComponent = newButton.GetComponentInChildren<TMP_Text>();
        	if (textComponent != null)
            {
                if (entry.isDiscovered)
                {
                    textComponent.text = entry.itemName;
                    textComponent.color = Color.white;
                }
                else
                {
                    textComponent.text = "???";
                    textComponent.color = new Color32(90, 90, 90, 255);
                }
            }

        	UIButtonHoverEffect hoverEffect = newButton.AddComponent<UIButtonHoverEffect>();
        	hoverEffect.Setup(this, newButton);

        	Button btn = newButton.GetComponent<Button>();
        	if (btn != null)
        	{
            	bool isDiscovered = entry.isDiscovered;
            	btn.interactable = isDiscovered;

            	btn.onClick.AddListener(() =>
            	{
                	if (!isDiscovered) return;

                	OnButtonClicked(entry, hoverEffect);
            	});
        	}
    	}
	}

	public void RefreshButtons()
	{
    	foreach (Transform child in contentParent)
        	Destroy(child.gameObject);

    	lastSelected = null;
    	GenerateButtons();
	}
    
    public void PlayHover()
    {
        if (hoverSound != null)
            hoverSound.Play();
    }

    void OnButtonClicked(LoreEntry entry, UIButtonHoverEffect clickedEffect)
	{
    	if (lastSelected != null && lastSelected != clickedEffect)
        	lastSelected.ResetVisual();

    	lastSelected = clickedEffect;
    	clickedEffect.ApplySelectedState();

    	previewManager.ShowPreview(entry.prefab);
    	contentEntrance.SetContentText(entry.loreText);
	}

    
    public void ResetSelectedButton()
    {
        if (lastSelected != null)
        {
            lastSelected.ResetVisual();
            lastSelected = null;
        }
    }
}

public class UIButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Image buttonImage;
    private Color originalColor;

    private ListButtonGenerator manager;

    private float scaleMultiplier = 1.15f;
    private Color hoverColor = new Color32(0x55, 0x00, 0x00, 200);
    private Color selectedColor = new Color32(0x55, 0x00, 0x00, 255);

    private bool isSelected = false;

    public void Setup(ListButtonGenerator mgr, GameObject button)
    {
        manager = mgr;
        originalScale = button.transform.localScale;

        buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
            originalColor = buttonImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        transform.localScale = originalScale * scaleMultiplier;
        if (buttonImage != null)
            buttonImage.color = hoverColor;

        manager?.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected) return;

        transform.localScale = originalScale;
        if (buttonImage != null)
            buttonImage.color = originalColor;
    }

    public void ApplySelectedState()
    {
        isSelected = true;
        transform.localScale = originalScale * scaleMultiplier;
        if (buttonImage != null)
            buttonImage.color = selectedColor;
    }

    public void ResetVisual()
    {
        isSelected = false;
        transform.localScale = originalScale;
        if (buttonImage != null)
            buttonImage.color = originalColor;
    }
}
