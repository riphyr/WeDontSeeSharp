using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviourPun
{
    private Dictionary<string, float> inventory = new Dictionary<string, float>();
    private PhotonView view;
    private PlayerUsing playerUsing;
    
    [Header("Affichage de l'inventaire")]
    public Image selectedItemIcon;
    
    [Header("Affichage des actions")]
    public TextMeshProUGUI itemActionText;
    

    private Dictionary<string, Sprite> itemSprites = new Dictionary<string, Sprite>();
    private List<string> inventoryKeys = new List<string>();
    private int selectedItemIndex = -1;

    void Start()
    {
        playerUsing = GetComponent<PlayerUsing>();
        
        LoadItemSprites();
        UpdateSelectedItemDisplay();
    }
    
    private void LoadItemSprites()
    {
        Debug.Log("Chargement des sprites d'inventaire...");
        Sprite[] sprites = Resources.LoadAll<Sprite>("InventorySprites");

        foreach (Sprite sprite in sprites)
        {
            itemSprites[sprite.name] = sprite;
            Debug.Log($"Sprite chargé : {sprite.name}");
        }
    }

    public bool HasItem(string itemName)
    {
        return inventory != null && inventory.ContainsKey(itemName) && inventory[itemName] > 0;
    }

    public float GetItemCount(string itemName)
    {
        return inventory.ContainsKey(itemName) ? inventory[itemName] : 0;
    }

    public void AddItem(string itemName, float amount = 1f)
    {
        if (amount < 0) return;

        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] += amount;
        }
        else
        {
            inventory[itemName] = amount;
            inventoryKeys.Add(itemName);
        }

        if (selectedItemIndex == -1)
        {
            selectedItemIndex = 0;
        }

        Debug.Log($"Ajout de {amount}x {itemName}. Nouveau total : {inventory[itemName]}");
        UpdateSelectedItemDisplay();
    }
    
    public bool RemoveItem(string itemName, float amount = 1f)
    {
        if (!inventory.ContainsKey(itemName) || inventory[itemName] < amount)
        {
            Debug.LogWarning($"Impossible de retirer {amount}x {itemName}, quantité insuffisante.");
            return false;
        }

        inventory[itemName] -= amount;
        if (inventory[itemName] <= 0)
        {
            inventory.Remove(itemName);
            inventoryKeys.Remove(itemName);
            Debug.Log($"{amount} x {itemName} retiré de l'inventaire.");
        }

        UpdateSelectedItemDisplay();
        return true;
    }
    
    public void ClearItem(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory.Remove(itemName);
            inventoryKeys.Remove(itemName);
            Debug.Log($"{itemName} supprimé de l'inventaire.");
        }
    }

    public void PrintInventory()
    {
        Debug.Log("Inventaire du joueur :");
        foreach (var item in inventory)
        {
            Debug.Log($"{item.Key} : {item.Value}");
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        inventoryKeys.Clear();
        Debug.Log("Inventaire vidé.");
    }
    
    public void SwitchToNextItem()
    {
        if (inventoryKeys.Count == 0)
        {
            Debug.LogWarning("Aucun objet dans l'inventaire !");
            return;
        }
        
        string currentItem = GetSelectedItem();
        if (!string.IsNullOrEmpty(currentItem) && playerUsing.IsItemEquipped(currentItem))
        {
            playerUsing.ForceUnequipItem(currentItem);
        }

        selectedItemIndex = (selectedItemIndex + 1) % inventoryKeys.Count;
        Debug.Log($"Changement d'objet : {inventoryKeys[selectedItemIndex]}");
        UpdateSelectedItemDisplay();
    }

    public void SwitchToPreviousItem()
    {
        if (inventoryKeys.Count == 0)
        {
            Debug.LogWarning("Aucun objet dans l'inventaire !");
            return;
        }
        
        string currentItem = GetSelectedItem();
        if (!string.IsNullOrEmpty(currentItem) && playerUsing.IsItemEquipped(currentItem))
        {
            playerUsing.ForceUnequipItem(currentItem);
        }

        selectedItemIndex = (selectedItemIndex - 1 + inventoryKeys.Count) % inventoryKeys.Count;
        Debug.Log($"Changement d'objet : {inventoryKeys[selectedItemIndex]}");
        UpdateSelectedItemDisplay();
    }

	public string GetSelectedItem()
	{
    	if (selectedItemIndex >= 0 && selectedItemIndex < inventoryKeys.Count)
    	{
        	return inventoryKeys[selectedItemIndex];
    	}
    	return null;
	}
    
    private void UpdateSelectedItemDisplay()
    {
        if (selectedItemIndex >= 0 && selectedItemIndex < inventoryKeys.Count)
        {
            string selectedItem = inventoryKeys[selectedItemIndex];
            if (itemSprites.ContainsKey(selectedItem))
            {
                selectedItemIcon.sprite = itemSprites[selectedItem]; 
                selectedItemIcon.enabled = true;
            }
            else
            {
                selectedItemIcon.enabled = false;
            }
        }
        else
        {
            selectedItemIcon.enabled = false;
        }
        
        UpdateActionText();
    }
    
    public void SetItemCount(string itemName, float amount)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] = amount;
        }
        else
        {
            inventory.Add(itemName, amount);
        }
    }
    
    public void UpdateActionText()
    {
        if (selectedItemIndex < 0 || selectedItemIndex >= inventoryKeys.Count)
        {
            itemActionText.text = "";
            return;
        }

        string selectedItem = inventoryKeys[selectedItemIndex];
        string useAction = "";
        string reloadAction = "";

        bool isEquipped = playerUsing.IsItemEquipped(selectedItem);

        switch (selectedItem)
        {
            case "Match":
                useAction = "Use";
                break;
            case "Candle":
                useAction = "Place";
                break;
            case "Flashlight":
                useAction = isEquipped ? "Stow" : "Take";
                reloadAction = HasItem("Flashlight") ? "" : "Recharge";
                break;
            case "UVFlashlight":
                useAction = isEquipped ? "Stow" : "Take";
                reloadAction = HasItem("UVFlashlight") ? "" : "Recharge";
                break;
            case "Wrench":
                useAction = isEquipped ? "Stow" : "Take";
                break;
            case "Crowbar":
                useAction = isEquipped ? "Stow" : "Take";
                break;
            case "Magnetophone":
                useAction = isEquipped ? "Stow" : "Take";
                break;
            case "EMFDetector":
                useAction = isEquipped ? "Stow" : "Take";
                break;
            case "CDDisk":
                useAction = isEquipped ? "Stow" : "Take";
                break;
            default:
                itemActionText.text = "";
                return;
        }

        string useKey = PlayerPrefs.GetString("Use", "None");
        string reloadKey = PlayerPrefs.GetString("Reload", "None");

        itemActionText.text = !string.IsNullOrEmpty(reloadAction) 
            ? $"{useAction} [{useKey}]\n{reloadAction} [{reloadKey}]"
            : $"{useAction} [{useKey}]";
    }
}