using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public string displayName;
    public string description;
    [TextArea] public string extraTooltip;
}

[CreateAssetMenu(menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items;

    public ItemData GetData(string itemName)
    {
        return items.Find(item => item.itemName == itemName);
    }
}
