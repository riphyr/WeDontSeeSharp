using UnityEngine;

[System.Serializable]
public class LoreEntry
{
    public string itemName;
    public GameObject prefab;
    [TextArea(3, 10)] public string loreText;

    [HideInInspector] public bool isDiscovered;

#if UNITY_EDITOR
    public void Reveal()
    {
        isDiscovered = true;
    }

    public void Hide()
    {
        isDiscovered = false;
    }
#endif
}