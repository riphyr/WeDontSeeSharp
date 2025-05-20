using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviourPun
{
    private Dictionary<string, float> inventory = new Dictionary<string, float>();
    private PlayerUsing playerUsing;
    private PhotonView photonView;
    
    [Header("Affichage de l'inventaire")]
    public Image selectedItemIcon;
    
    [Header("Affichage des actions")]
    public TextMeshProUGUI itemActionText;
    

    private Dictionary<string, Sprite> itemSprites = new Dictionary<string, Sprite>();
    private List<string> inventoryKeys = new List<string>();
    private int selectedItemIndex = -1;
    private readonly HashSet<string> slotItems = new HashSet<string>
    {
        "Match",
        "Candle",
        "Flashlight",
        "UVFlashlight",
        "Wrench",
        "Crowbar",
        "Magnetophone",
        "EMFDetector",
        "CDDisk"
    };


    void Start()
    {
        playerUsing = GetComponent<PlayerUsing>();
        photonView = GetComponent<PhotonView>();
        
        LoadItemSprites();
        UpdateSelectedItemDisplay();
    }
    
    private void LoadItemSprites()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("InventorySprites");

        foreach (Sprite sprite in sprites)
        {
            itemSprites[sprite.name] = sprite;
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
        if (amount <= 0) return;
    
        if (!inventory.ContainsKey(itemName))
        {
            inventory[itemName] = 0;
            if (slotItems.Contains(itemName))
            {
                inventoryKeys.Add(itemName);
            }
        }

        inventory[itemName] += amount;

        if (selectedItemIndex == -1 && inventoryKeys.Count > 0)
        {
            selectedItemIndex = 0;
        }

        UpdateSelectedItemDisplay();
    }
    
    public bool RemoveItem(string itemName, float amount = 1f)
    {
        if (!inventory.ContainsKey(itemName) || inventory[itemName] < amount)
        {
            return false;
        }

        inventory[itemName] -= amount;

        if (inventory[itemName] <= 0)
        {
            inventory.Remove(itemName);
        
            int removedIndex = inventoryKeys.IndexOf(itemName);
            if (removedIndex >= 0)
            {
                inventoryKeys.RemoveAt(removedIndex);
                if (removedIndex == selectedItemIndex)
                {
                    if (inventoryKeys.Count == 0) selectedItemIndex = -1;
                    else if (removedIndex >= inventoryKeys.Count)
                        selectedItemIndex = inventoryKeys.Count - 1;
                    else
                        selectedItemIndex = removedIndex;
                }
                else if (removedIndex < selectedItemIndex)
                {
                    selectedItemIndex--;
                }
            }
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
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        inventoryKeys.Clear();
    }
    
    public void SwitchToNextItem()
    {
        if (!photonView.IsMine) return;
        if (inventoryKeys.Count == 0)
        {
            return;
        }
    
        string currentItem = GetSelectedItem();
        if (!string.IsNullOrEmpty(currentItem) && playerUsing.IsItemEquipped(currentItem))
        {
            playerUsing.ForceUnequipItem(currentItem);
        }

        selectedItemIndex = (selectedItemIndex + 1) % inventoryKeys.Count;
        UpdateSelectedItemDisplay();
    }

    public void SwitchToPreviousItem()
    {
        if (!photonView.IsMine) return;

        if (inventoryKeys.Count == 0)
        {
            return;
        }
    
        string currentItem = GetSelectedItem();
        if (!string.IsNullOrEmpty(currentItem) && playerUsing.IsItemEquipped(currentItem))
        {
            playerUsing.ForceUnequipItem(currentItem);
        }

        selectedItemIndex = (selectedItemIndex - 1 + inventoryKeys.Count) % inventoryKeys.Count;
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

	public List<string> GetAllInventoryKeys()
	{
    	return new List<string>(inventory.Keys);
	}

	public Sprite GetSprite(string itemName)
	{
    	return itemSprites.TryGetValue(itemName, out Sprite sprite) ? sprite : null;
	}

	public void ForceSelectItem(string itemName)
	{
    	int index = GetAllInventoryKeys().IndexOf(itemName);
    	if (index != -1)
    	{
        	typeof(PlayerInventory)
            	.GetField("selectedItemIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            	?.SetValue(this, index);
    	}
	}

	public void SwitchToItemDirectly(string targetItem)
    {
        if (!photonView.IsMine) return;

        if (inventoryKeys.Count == 0)
        {
            return;
        }

        int targetIndex = inventoryKeys.IndexOf(targetItem);

        if (targetIndex == -1)
        {
            return;
        }

        string currentItem = GetSelectedItem();

        if (!string.IsNullOrEmpty(currentItem) && playerUsing.IsItemEquipped(currentItem))
        {
            playerUsing.ForceUnequipItem(currentItem);
        }

        selectedItemIndex = targetIndex;

        UpdateSelectedItemDisplay();
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