using UnityEngine;

[CreateAssetMenu(fileName = "LoreDatabase", menuName = "Journal/Lore Database")]
public class LoreDatabase : ScriptableObject
{
    public LoreEntry[] loreEntries;
	public ListButtonGenerator listButtonGenerator;

    public void MarkAsDiscovered(string itemName)
    {
        foreach (var entry in loreEntries)
        {
            if (entry.itemName == itemName && !entry.isDiscovered)
            {
                entry.isDiscovered = true;

                if (listButtonGenerator != null)
                    listButtonGenerator.RefreshButtons();

                break;
            }
        }
    }

    public bool IsDiscovered(string itemName)
    {
        foreach (var entry in loreEntries)
        {
            if (entry.itemName == itemName)
                return entry.isDiscovered;
        }
        return false;
    }
}
