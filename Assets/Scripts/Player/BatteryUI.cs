using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BatteryUI : MonoBehaviour
{
    [Header("Références")]
    public PlayerInventory inventory;
    public PlayerUsing playerUsing;
    public Slider batterySlider;
    public Image fillImage;

    [Header("Paramètres")]
    public List<string> batteryItems = new List<string> { "Flashlight", "UVFlashlight" };

    void Update()
    {
        string selectedItem = inventory.GetSelectedItem();
        bool showUI = !string.IsNullOrEmpty(selectedItem) && batteryItems.Contains(selectedItem);
        SetChildrenActive(showUI);

        if (!showUI) return;

        float batteryPercent = 0;

        if (selectedItem == "Flashlight" && playerUsing.GetEquippedFlashlight() != null)
            batteryPercent = playerUsing.GetEquippedFlashlight().GetCurrentBattery();
        else if (selectedItem == "UVFlashlight" && playerUsing.GetEquippedUVFlashlight() != null)
            batteryPercent = playerUsing.GetEquippedUVFlashlight().GetCurrentBattery();
        else
            batteryPercent = inventory.GetItemCount(selectedItem);

        if (batteryPercent <= 0f)
        {
            batterySlider.value = 100f;
            fillImage.color = Color.red;
        }
        else if (batteryPercent >= 100f)
        {
            batterySlider.value = 100f;
            fillImage.color = Color.green;
        }
        else
        {
            batterySlider.value = batteryPercent;
            fillImage.color = Color.gray;
        }
    }

    private void SetChildrenActive(bool state)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(state);
        }
    }
}