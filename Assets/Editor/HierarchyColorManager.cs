using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class HierarchyColorManager : EditorWindow
{
    private Vector2 scrollPos;
    private string searchQuery = "";
    private string sortMode = "Name";
    private static readonly string[] sortOptions = { "Name", "Color" };
    private static readonly string[] colorOptions = { "None", "Red", "Green", "Blue", "Purple", "Yellow" };

    private static string hiddenColorFilter = null;
    public static string HiddenFilter => hiddenColorFilter;

    [MenuItem("Tools/Hierarchy Color Manager")]
    public static void OpenWindow()
    {
        GetWindow<HierarchyColorManager>("Hierarchy Color Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("🎨 Colored GameObjects in Scene", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        searchQuery = EditorGUILayout.TextField("Search", searchQuery);
        sortMode = sortOptions[EditorGUILayout.Popup(System.Array.IndexOf(sortOptions, sortMode), sortOptions, GUILayout.Width(100))];
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        var coloredObjects = new List<(GameObject obj, string color)>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags != HideFlags.None || EditorUtility.IsPersistent(obj))
                continue;

            string key = $"HierarchyColor_{obj.GetInstanceID()}";
            string colorName = EditorPrefs.GetString(key, "None");

            if (colorName != "None" && obj.name.ToLower().Contains(searchQuery.ToLower()))
            {
                coloredObjects.Add((obj, colorName));
            }
        }

        if (sortMode == "Name")
            coloredObjects = coloredObjects.OrderBy(e => e.obj.name).ToList();
        else
            coloredObjects = coloredObjects.OrderBy(e => e.color).ThenBy(e => e.obj.name).ToList();

        foreach (var (obj, colorName) in coloredObjects)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.color = GetColor(colorName);
            GUILayout.Label("■", GUILayout.Width(20));
            GUI.color = Color.white;

            GUILayout.Label(obj.name, GUILayout.Width(180));

            if (GUILayout.Button("🎯 Focus", GUILayout.Width(60)))
            {
                Selection.activeGameObject = obj;
                EditorGUIUtility.PingObject(obj);
            }

            int currentIndex = System.Array.IndexOf(colorOptions, colorName);
            int selectedIndex = EditorGUILayout.Popup(currentIndex, colorOptions, GUILayout.Width(80));
            string newColor = colorOptions[selectedIndex];
            if (newColor != colorName)
            {
                string key = $"HierarchyColor_{obj.GetInstanceID()}";
                EditorPrefs.SetString(key, newColor);
                Repaint();
                EditorApplication.RepaintHierarchyWindow();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        GUILayout.Label("📂 Expand by Color", EditorStyles.boldLabel);
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
        GUILayout.Label("👁 Show Only by Color", EditorStyles.boldLabel);
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
            if (GUILayout.Button("🔁 Show All"))
            {
                hiddenColorFilter = null;
                UnityEditor.SceneVisibilityManager.instance.ShowAll();
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("🧹 Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Clear All Color Tags"))
        {
            if (EditorUtility.DisplayDialog("Clear All Colors?",
                "This will remove ALL hierarchy color tags from ALL objects in the scene.\nAre you sure?",
                "Yes, clear everything", "Cancel"))
            {
                foreach (var (obj, _) in coloredObjects)
                {
                    EditorPrefs.DeleteKey($"HierarchyColor_{obj.GetInstanceID()}");
                }

                Repaint();
                EditorApplication.RepaintHierarchyWindow();
            }
        }
    }

    static void ExpandAllColor(string colorTag)
    {
        HierarchyColorFilter.ShowColor(colorTag);
    }
    
    static void ShowOnlyObjectsWithColor(string color)
    {
        var visibilityManager = UnityEditor.SceneVisibilityManager.instance;
        visibilityManager.ShowAll();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        HashSet<GameObject> keepVisible = new HashSet<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags != HideFlags.None || EditorUtility.IsPersistent(obj))
                continue;

            string key = $"HierarchyColor_{obj.GetInstanceID()}";
            string colorName = EditorPrefs.GetString(key, "None");

            if (colorName == color)
            {
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
        }

        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags != HideFlags.None || EditorUtility.IsPersistent(obj))
                continue;

            if (!keepVisible.Contains(obj))
            {
                visibilityManager.Hide(obj, false);
            }
        }

        hiddenColorFilter = color;
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
            _ => Color.white
        };
    }
}