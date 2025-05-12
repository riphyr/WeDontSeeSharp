using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HierarchyColorManager : EditorWindow
{
    private Vector2 scrollPos;
    private string searchQuery = "";
    private string sortMode = "Name";
    private static readonly string[] sortOptions = { "Name", "Color" };
    private static readonly string[] colorOptions = { "None", "Red", "Green", "Blue", "Purple", "Yellow", "Magenta" };

    private static string hiddenColorFilter = null;
    public static string HiddenFilter => hiddenColorFilter;
	private List<(GameObject obj, string color)> cachedColoredObjects = new List<(GameObject, string)>();

    [MenuItem("Tools/Color Manager")]
    public static void OpenWindow()
    {
        GetWindow<HierarchyColorManager>("Color Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("Colored GameObjects in Scene", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        searchQuery = EditorGUILayout.TextField("Search", searchQuery);
        sortMode = sortOptions[EditorGUILayout.Popup(System.Array.IndexOf(sortOptions, sortMode), sortOptions, GUILayout.Width(100))];
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        var filteredObjects = cachedColoredObjects
            .Where(e => e.obj != null && e.obj.name.ToLower().Contains(searchQuery.ToLower()))
            .ToList();

        if (sortMode == "Name")
            filteredObjects = filteredObjects.OrderBy(e => e.obj.name).ToList();
        else
            filteredObjects = filteredObjects.OrderBy(e => e.color).ThenBy(e => e.obj.name).ToList();

        foreach (var (obj, colorName) in filteredObjects)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.color = GetColor(colorName);
            GUILayout.Label("■", GUILayout.Width(20));
            GUI.color = Color.white;

            GUILayout.Label(obj.name, GUILayout.Width(180));

            if (GUILayout.Button("Focus", GUILayout.Width(60)))
            {
                Selection.activeGameObject = obj;
                EditorGUIUtility.PingObject(obj);
            }

            int currentIndex = System.Array.IndexOf(colorOptions, colorName);
            int selectedIndex = EditorGUILayout.Popup(currentIndex, colorOptions, GUILayout.Width(80));
            string newColor = colorOptions[selectedIndex];
            if (newColor != colorName)
            {
                HierarchyColorDatabase.Instance.SetColor(obj, newColor);
                RefreshCache();
                Repaint();
                EditorApplication.RepaintHierarchyWindow();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        GUILayout.Label("Expand by Color", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        foreach (var color in colorOptions)
        {
            if (color == "None") continue;
            if (GUILayout.Button(color, GUILayout.Width(60)))
            {
                ExpandAllColor(color);
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Show Only by Color", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        foreach (var color in colorOptions)
        {
            if (color == "None") continue;
            if (GUILayout.Button(color, GUILayout.Width(60)))
            {
                ShowOnlyObjectsWithColor(color);
            }
        }
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(hiddenColorFilter))
        {
            if (GUILayout.Button("Show All"))
            {
                hiddenColorFilter = null;
                SceneVisibilityManager.instance.ShowAll();
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Clear All Color Tags"))
        {
            if (EditorUtility.DisplayDialog("Clear All Colors?",
                "This will remove ALL hierarchy color tags from ALL objects in the scene.\nAre you sure?",
                "Yes, clear everything", "Cancel"))
            {
                HierarchyColorDatabase.Instance.ClearAll();
                RefreshCache();
                Repaint();
                EditorApplication.RepaintHierarchyWindow();
            }
        }

		GUILayout.Space(10);
		GUILayout.Label("Cache", EditorStyles.boldLabel);

		if (GUILayout.Button("Refresh List"))
		{
    		RefreshCache();
    		Repaint();
		}
    }

    static void ExpandAllColor(string colorTag)
    {
        HierarchyColorFilter.ShowColor(colorTag);
    }

    static void ShowOnlyObjectsWithColor(string color)
    {
        var visibilityManager = SceneVisibilityManager.instance;
        visibilityManager.ShowAll();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        HashSet<GameObject> keepVisible = new HashSet<GameObject>();

        foreach (var (obj, col) in HierarchyColorDatabase.Instance.GetAllColoredObjects())
        {
            if (obj.hideFlags != HideFlags.None || EditorUtility.IsPersistent(obj)) continue;
            if (col != color) continue;

            keepVisible.Add(obj);

            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                keepVisible.Add(parent.gameObject);
                parent = parent.parent;
            }

            foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
            {
                keepVisible.Add(child.gameObject);
            }
        }

        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags != HideFlags.None || EditorUtility.IsPersistent(obj)) continue;
            if (!keepVisible.Contains(obj))
            {
                visibilityManager.Hide(obj, false);
            }
        }

        hiddenColorFilter = color;
    }

	private void OnEnable()
	{
    	RefreshCache();
	}

	private void RefreshCache()
	{
    	cachedColoredObjects = HierarchyColorDatabase.Instance.GetAllColoredObjects()
        	.Where(e => e.obj != null)
        	.ToList();
	}

    static Color GetColor(string name)
    {
        return name switch
        {
            "Red" => new Color(1f, 0.4f, 0.4f),
            "Green" => new Color(0.5f, 1f, 0.5f),
            "Blue" => new Color(0.5f, 0.7f, 1f),
            "Purple" => new Color(0.8f, 0.6f, 1f),
            "Yellow" => new Color(1f, 1f, 0.5f),
            "Magenta" => new Color(1f, 0.5f, 1f),
            _ => Color.white
        };
    }
}
