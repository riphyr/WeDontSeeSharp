using UnityEngine;

public class LoreDatabaseLinker : MonoBehaviour
{
    [Header("Références")]
    public LoreDatabase loreDatabase;
    public ListButtonGenerator buttonGenerator;

    void Awake()
    {
        if (loreDatabase != null && buttonGenerator != null)
        {
            loreDatabase.listButtonGenerator = buttonGenerator;
        }
    }
}