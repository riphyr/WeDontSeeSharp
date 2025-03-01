using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInventory : MonoBehaviourPun
{
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    private PhotonView view;

    void Start()
    { }

    public bool HasItem(string itemName)
    {
        return inventory != null && inventory.ContainsKey(itemName) && inventory[itemName] > 0;
    }

    public int GetItemCount(string itemName)
    {
        return inventory.ContainsKey(itemName) ? inventory[itemName] : 0;
    }

    public void AddItem(string itemName, int amount = 1)
    {
        if (amount <= 0) 
            return;

        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] += amount;
        }
        else
        {
            inventory[itemName] = amount;
        }
    }
    
    public bool RemoveItem(string itemName, int amount = 1)
    {
        if (!inventory.ContainsKey(itemName) || inventory[itemName] < amount)
        {
            return false;
        }

        inventory[itemName] -= amount;
        if (inventory[itemName] <= 0)
        {
            inventory.Remove(itemName);
        }

        return true;
    }
    
    public void ClearItem(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory.Remove(itemName);
        }
    }

    public void PrintInventory()
    {
        Debug.Log("👜 Inventaire du joueur :");
        foreach (var item in inventory)
        {
            Debug.Log($"{item.Key} : {item.Value}");
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
    }
}
