using UnityEngine;

[CreateAssetMenu(fileName = "LoreDatabase", menuName = "Journal/Lore Database")]
public class LoreDatabase : ScriptableObject
{
    public LoreEntry[] loreEntries;

    public void MarkAsDiscovered(string itemName)
    {
        foreach (var entry in loreEntries)
        {
            if (entry.itemName == itemName && !entry.isDiscovered)
            {
                entry.isDiscovered = true;

                // Rafraîchir tous les boutons actifs dans la scène
                var allGenerators = GameObject.FindObjectsOfType<ListButtonGenerator>();
                foreach (var gen in allGenerators)
                    gen.RefreshButtons();

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
    
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnEnterPlayMode]
    static void OnEnterPlaymodeResetData()
    {
        var db = Resources.Load<LoreDatabase>("LoreDatabase");
        foreach (var entry in db.loreEntries)
        {
            entry.isDiscovered = false; // Reset automatique de tout
        }
    }
    #endif
}
