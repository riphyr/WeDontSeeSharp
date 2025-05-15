using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("Références principales")]
    [SerializeField] private PlayerInventory playerInventory; 
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TextMeshProUGUI informationText;

    [Header("Boutons")]
    [SerializeField] private Button dropItemButton;
    [SerializeField] private Button dropStackButton;
	[SerializeField] private GameObject dropItemHighlightLine;
	[SerializeField] private GameObject dropStackHighlightLine;
	private Vector3 dropItemOriginalScale;
	private Vector3 dropStackOriginalScale;

    [Header("Slots d'inventaire (visibles)")]
    [SerializeField] private List<InventorySlotUI> slotUIs;
    [SerializeField] private ItemDatabase itemDatabase;

	[Header("Paramètres de double clique")]
	[SerializeField] private float lastClickTime = 0f;
	[SerializeField] private const float doubleClickThreshold = 0.25f;

    [Header("Slider de pagination")]
    [SerializeField] private Slider inventorySlider;

	[Header("Menus à désactiver")]
	[SerializeField] private PlayerJournalUI playerJournalUI;
	[SerializeField] private PlayerScript playerScript;
	[SerializeField] private PauseMenu.PauseMenuManager pauseMenuManager;
	[SerializeField] private PlayerUsing playerUsing;
	[SerializeField] private CameraLookingAt cameraLookingAt;

    private bool isInventoryOpen = false;
    private const int ITEMS_PER_PAGE = 25;
    private int currentPage = 0;
	private string selectedItemName = null;
	private KeyCode inventoryKey;

    private void Start()
    {
        HideInventory();

        if (dropItemButton)
		{
			dropItemButton.onClick.AddListener(OnDropItem);
			dropItemOriginalScale = dropItemButton.transform.localScale;
			AddButtonScaleEvents(dropItemButton, dropItemOriginalScale);
		}
        if (dropStackButton) 
		{
			dropStackButton.onClick.AddListener(OnDropStack);
			dropStackOriginalScale = dropStackButton.transform.localScale;
			AddButtonScaleEvents(dropStackButton, dropStackOriginalScale);
		}


        if (inventorySlider)
        {
            inventorySlider.wholeNumbers = true;
            inventorySlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
        {
            if (isInventoryOpen) 
                HideInventory();
            else                 
                ShowInventory();
        }
		else if (isInventoryOpen)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				HideInventory();
		}

		LoadInventoryKey();
    }

	private KeyCode GetKeyCodeFromString(string key)
    {
        return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
    }

	private void LoadInventoryKey()
    {
		string key = PlayerPrefs.GetString("Inventory", "I");
        inventoryKey = GetKeyCodeFromString(key);
    }

    public void ShowInventory()
    {
        isInventoryOpen = true;
        if (inventoryPanel) 
			inventoryPanel.SetActive(true);

		if (playerJournalUI) 
			playerJournalUI.enabled = false;
		if (playerScript) 
			playerScript.enabled = false;
    	if (pauseMenuManager) 
			pauseMenuManager.enabled = false;
		if (playerUsing) 
			playerUsing.enabled = false;
    	if (cameraLookingAt) 
			cameraLookingAt.enabled = false;

    	Cursor.lockState = CursorLockMode.None;
    	Cursor.visible = true;		

        currentPage = 0;
        RefreshSliderRange();
        RefreshInventorySlots();
        informationText.text = "";
    }

    public void HideInventory()
    {
        isInventoryOpen = false;
        if (inventoryPanel) 
            inventoryPanel.SetActive(false);
	
		if (playerJournalUI) 
			playerJournalUI.enabled = true;
		if (playerScript) 
			playerScript.enabled = true;
    	if (pauseMenuManager) 
			pauseMenuManager.enabled = true;
		if (playerUsing) 
			playerUsing.enabled = true;
    	if (cameraLookingAt) 
			cameraLookingAt.enabled = true;

    	Cursor.lockState = CursorLockMode.Locked;
    	Cursor.visible = false;	
    }

    private void RefreshSliderRange()
    {
        if (!inventorySlider) 
            return;

        List<string> allKeys = playerInventory.GetAllInventoryKeys();
        int pageCount = Mathf.CeilToInt(allKeys.Count / (float)ITEMS_PER_PAGE);

        inventorySlider.minValue = 0;
        inventorySlider.maxValue = Mathf.Max(0, pageCount - 1);
        inventorySlider.value = 0;
    }

    private void OnSliderValueChanged(float newValue)
    {
        currentPage = (int)newValue;
        RefreshInventorySlots();
    }

    private void RefreshInventorySlots()
    {
        List<string> allKeys = playerInventory.GetAllInventoryKeys();
        int startIndex = currentPage * ITEMS_PER_PAGE;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            int itemIndex = startIndex + i;
            if (itemIndex < allKeys.Count)
            {
                string itemName = allKeys[itemIndex];
                float itemCount = playerInventory.GetItemCount(itemName);
                slotUIs[i].SetSlotData(itemName, itemCount, this);
            }
            else
            {
                slotUIs[i].ClearSlot();
            }
        }
    }
    
    public void OnSlotClicked(string itemName)
    {
	    float timeSinceLastClick = Time.time - lastClickTime;
	    lastClickTime = Time.time;

	    selectedItemName = itemName;
	    if (informationText)
	    {
		    ItemData data = itemDatabase.GetData(itemName);
		    string display = $"Item : {data.displayName}\nAmount : {playerInventory.GetItemCount(itemName)}\n\nDescription : {data.description}";

		    if (PlayerPrefs.GetInt("ToolTips", 1) == 1 && !string.IsNullOrWhiteSpace(data.extraTooltip))
			    display += $"\n\nTooltips : {data.extraTooltip}";

		    informationText.text = display;
	    }

	    if (timeSinceLastClick <= doubleClickThreshold)
		    playerInventory.SwitchToItemDirectly(itemName);
    }


    public void OnSlotHoverEnter(int slotIndex)
    {
        slotUIs[slotIndex].Highlight(true);
    }

    public void OnSlotHoverExit(int slotIndex)
    {
        slotUIs[slotIndex].Highlight(false);
    }

    private void OnDropItem()
    {
        if (!string.IsNullOrEmpty(selectedItemName) && playerUsing != null)
    	{
        	playerUsing.DropItemByName(selectedItemName, false);
			RefreshUI();
    	}
    	else
    	{
        	Debug.LogWarning("Aucun item sélectionné ou référence à PlayerUsing manquante !");
    	}
    }

    private void OnDropStack()
    {
        if (!string.IsNullOrEmpty(selectedItemName) && playerUsing != null)
    	{
        	playerUsing.DropItemByName(selectedItemName, true);
			RefreshUI();
    	}
    	else
    	{
        	Debug.LogWarning("Aucun item sélectionné ou référence à PlayerUsing manquante !");
    	}
    }

	private void AddButtonScaleEvents(Button button, Vector3 originalScale)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener((data) =>
        {
            button.transform.localScale = originalScale * 0.95f;
        });

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUpEntry.callback.AddListener((data) =>
        {
            button.transform.localScale = originalScale;
        });

        trigger.triggers.Add(pointerDownEntry);
        trigger.triggers.Add(pointerUpEntry);
    }

	public void RefreshUI()
	{
    	RefreshSliderRange();
    	RefreshInventorySlots();
    	informationText.text = "";
	}

	public void OnDropItemHoverEnter()  => dropItemHighlightLine?.SetActive(true);
	public void OnDropItemHoverExit()   => dropItemHighlightLine?.SetActive(false);

	public void OnDropStackHoverEnter() => dropStackHighlightLine?.SetActive(true);
	public void OnDropStackHoverExit()  => dropStackHighlightLine?.SetActive(false);

    public Sprite GetSpriteForItem(string itemName)
    {
        return playerInventory.GetSprite(itemName);
    }
}
