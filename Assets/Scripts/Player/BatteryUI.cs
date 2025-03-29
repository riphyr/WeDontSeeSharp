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
    Color criticalBatteryColor = new Color(125f / 255f, 0f, 0f);
    Color fullBatteryColor = new Color(0f, 118f / 255f, 0f);
    Color mediumBatteryColor = new Color(152f / 255f, 148f / 255f, 56f / 255f);

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

        if (batteryPercent == 0f)
        {
            batterySlider.value = 100f;
            fillImage.color = criticalBatteryColor;
        }
        else if (batteryPercent == 100f)
        {
            batterySlider.value = 100f;
            fillImage.color = fullBatteryColor;
        }
        else if (batteryPercent <= 25f)
        {
            batterySlider.value = batteryPercent;
            float t = batteryPercent / 25f;
            fillImage.color = Color.Lerp(criticalBatteryColor, mediumBatteryColor, t);
        }
        else if (batteryPercent >= 75f)
        {
            batterySlider.value = batteryPercent;
            float t = (batteryPercent - 75f) / 25f;
            fillImage.color = Color.Lerp(mediumBatteryColor, fullBatteryColor, t);
        }
        else
        {
            batterySlider.value = batteryPercent;
            fillImage.color = mediumBatteryColor;
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