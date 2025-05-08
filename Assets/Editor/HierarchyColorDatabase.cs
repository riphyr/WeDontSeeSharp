using UnityEngine; 
using UnityEditor; 
using System.Collections.Generic; 
using UnityEditor.SceneManagement; 
using UnityEditor.Experimental.SceneManagement; 
using UnityEditor.Callbacks;

[System.Serializable]
public class HierarchyColorEntry
{
    public string globalID; 
    public string color;
}

public class HierarchyColorDatabase : ScriptableObject
{
    private static HierarchyColorDatabase instance;
    public static string AssetPath => "Assets/Editor/HierarchyColorDatabase.asset";

    [SerializeField]
    private List<HierarchyColorEntry> colorEntries = new List<HierarchyColorEntry>();

    private Dictionary<string, string> cache;

    public static HierarchyColorDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = AssetDatabase.LoadAssetAtPath<HierarchyColorDatabase>(AssetPath);
                if (instance == null)
                {
                    instance = CreateInstance<HierarchyColorDatabase>();
                    AssetDatabase.CreateAsset(instance, AssetPath);
                    AssetDatabase.SaveAssets();
                }
                instance.RebuildCache();
            }
            return instance;
        }
    }

    public string GetColor(GameObject obj)
    {
        string globalID = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
        return Instance.cache.TryGetValue(globalID, out string color) ? color : "None";
    }

    public void SetColor(GameObject obj, string color)
    {
        string globalID = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
        if (Instance.cache.ContainsKey(globalID))
        {
            Instance.cache[globalID] = color;
            var entry = colorEntries.Find(e => e.globalID == globalID);
            entry.color = color;
        }
        else
        {
            Instance.cache[globalID] = color;
            colorEntries.Add(new HierarchyColorEntry { globalID = globalID, color = color });
        }
        EditorUtility.SetDirty(this);
    }

    public void ClearColor(GameObject obj)
    {
        string globalID = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
        cache.Remove(globalID);
        colorEntries.RemoveAll(e => e.globalID == globalID);
        EditorUtility.SetDirty(this);
    }

    public void ClearAll()
    {
        colorEntries.Clear();
        cache.Clear();
        EditorUtility.SetDirty(this);
    }

    public IEnumerable<(GameObject obj, string color)> GetAllColoredObjects()
    {
        foreach (var entry in colorEntries)
        {
            if (GlobalObjectId.TryParse(entry.globalID, out var id))
            {
                var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                if (obj != null)
                    yield return (obj, entry.color);
            }
        }
    }

    public void RebuildCache()
    {
        cache = new Dictionary<string, string>();
        foreach (var entry in colorEntries)
        {
            if (!cache.ContainsKey(entry.globalID))
                cache.Add(entry.globalID, entry.color);
        }
    }

    [DidReloadScripts]
    private static void OnReload()
    {
        if (instance != null)
            instance.RebuildCache();
    }
    
    [MenuItem("Tools/Hierarchy Color/Force Reload Colors")]
    public static void ForceReloadColors()
    {
        instance.RebuildCache();
        EditorApplication.RepaintHierarchyWindow();
    }
}


    

