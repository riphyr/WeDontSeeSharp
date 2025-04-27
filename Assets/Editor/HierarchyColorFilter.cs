using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;

[InitializeOnLoad]
public static class HierarchyColorFilter
{
    static HierarchyColorFilter()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    static void OnHierarchyGUI(int instanceID, Rect selectionRect) { }

    private static string FilterColor = "None";

    [MenuItem("Tools/Show All Colored/Red")]
    public static void ShowRed() => ShowColor("Red");

    [MenuItem("Tools/Show All Colored/Green")]
    public static void ShowGreen() => ShowColor("Green");

    [MenuItem("Tools/Show All Colored/Blue")]
    public static void ShowBlue() => ShowColor("Blue");

    [MenuItem("Tools/Show All Colored/Purple")]
    public static void ShowPurple() => ShowColor("Purple");

    [MenuItem("Tools/Show All Colored/Yellow")]
    public static void ShowYellow() => ShowColor("Yellow");

    public static void ShowColor(string colorTag)
    {
        FilterColor = colorTag;
        ExpandFilteredObjects(colorTag);
    }

    static void ExpandFilteredObjects(string colorTag)
    {
        List<int> expandedIDs = new List<int>();
        List<GameObject> toHighlight = new List<GameObject>();

        foreach (var (obj, color) in HierarchyColorDatabase.Instance.GetAllColoredObjects())
        {
            if (obj == null || color != colorTag) continue;

            toHighlight.Add(obj);

            Transform current = obj.transform;
            while (current != null)
            {
                expandedIDs.Add(current.gameObject.GetInstanceID());
                current = current.parent;
            }
        }

        var window = GetHierarchyWindow();
        if (window != null)
        {
            foreach (int id in expandedIDs)
            {
                SetExpanded(window, id, true);
            }
        }

        Selection.objects = toHighlight.ToArray();
    }

    static void SetExpanded(EditorWindow window, int instanceID, bool expand)
    {
        var sceneHierarchyType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var treeField = sceneHierarchyType.GetProperty("sceneHierarchy", BindingFlags.Instance | BindingFlags.NonPublic);
        var sceneHierarchy = treeField?.GetValue(window);

        var setExpandedMethod = sceneHierarchy?.GetType().GetMethod("ExpandTreeViewItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        setExpandedMethod?.Invoke(sceneHierarchy, new object[] { instanceID, expand });
    }

    static EditorWindow GetHierarchyWindow()
    {
        var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        return EditorWindow.GetWindow(windowType);
    }
}
