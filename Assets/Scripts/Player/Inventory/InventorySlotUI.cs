using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemCountText;

    private string currentItemName;
    private PlayerInventoryUI parentUI;
    public void SetSlotData(string itemName, float count, PlayerInventoryUI ui)
    {
        currentItemName = itemName;
        parentUI = ui;

        Sprite icon = parentUI.GetSpriteForItem(itemName);
        if (icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = true;
        }
        else
        {
            itemIcon.enabled = false;
        }

        if (itemName == "Flashlight" || itemName == "UVFlashlight")
            itemCountText.text = $"{Mathf.FloorToInt(count)} %";
        else
            itemCountText.text = $"{Mathf.FloorToInt(count)}";

        gameObject.SetActive(true);
    }

    public void ClearSlot()
    {
        currentItemName = null;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemCountText.text = "";
        gameObject.SetActive(false);
    }

    public void OnClickSlot()
    {
        if (!string.IsNullOrEmpty(currentItemName))
        {
            parentUI.OnSlotClicked(currentItemName);
        }
    }

    public void Highlight(bool show)
    {
        float scale = show ? 1.1f : 1f;

        itemIcon.rectTransform.localScale = new Vector3(scale, scale, 1f);
        itemCountText.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}
