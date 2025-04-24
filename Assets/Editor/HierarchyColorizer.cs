using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

[InitializeOnLoad]
public static class HierarchyColorizerLayered
{
    static Texture2D iconGray;
    static Texture2D iconBlue;
    static Texture2D iconRed;

    static HierarchyColorizerLayered()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

        iconGray = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
        iconBlue = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
        iconRed = EditorGUIUtility.IconContent("console.erroricon.sml").image as Texture2D;
    }

    static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        string key = $"HierarchyColor_{instanceID}";
        string colorName = EditorPrefs.GetString(key, "None");

        Color unityDarkGray = new Color(0.22f, 0.22f, 0.22f, 1f);
        EditorGUI.DrawRect(selectionRect, unityDarkGray);

        bool isSelected = System.Array.IndexOf(Selection.instanceIDs, instanceID) >= 0;

        bool isHovered = Event.current.type == EventType.Repaint && selectionRect.Contains(Event.current.mousePosition);

        if (isHovered && !isSelected && Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(selectionRect, new Color(1f, 1f, 1f, 0.06f));
        }
        else if (isSelected)
        {
            EditorGUI.DrawRect(selectionRect, new Color(0.173f, 0.365f, 0.529f, 1f));
        }

        Color textColor = colorName == "None" ? GUI.skin.label.normal.textColor : GetColor(colorName);
        if (!obj.activeInHierarchy)
            textColor *= 0.6f;


        var labelStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState { textColor = textColor }
        };

        Texture2D icon = GetIconForObject(obj);
        Rect iconRect = new Rect(selectionRect.x + 1, selectionRect.y + 1, 16, 16);
        if (icon) GUI.DrawTexture(iconRect, icon);

        Rect labelRect = new Rect(selectionRect);
        labelRect.x += 18f;
        labelRect.y -= 0.6f;
        EditorGUI.LabelField(labelRect, obj.name, labelStyle);
    }

    static Texture2D GetIconForObject(GameObject obj)
    {
        var type = PrefabUtility.GetPrefabAssetType(obj);
        var status = PrefabUtility.GetPrefabInstanceStatus(obj);

        if (status == PrefabInstanceStatus.MissingAsset)
            return iconRed;
        else if (status == PrefabInstanceStatus.Connected)
            return iconBlue;
        else
            return iconGray;
    }

    #region MENU CONTEXTUEL
    [MenuItem("GameObject/Set Hierarchy Color/None", false, -50)]
    static void ClearColor() => SetColor("None");

    [MenuItem("GameObject/Set Hierarchy Color/Red", false, -49)]
    static void SetRed() => SetColor("Red");

    [MenuItem("GameObject/Set Hierarchy Color/Green", false, -48)]
    static void SetGreen() => SetColor("Green");

    [MenuItem("GameObject/Set Hierarchy Color/Blue", false, -47)]
    static void SetBlue() => SetColor("Blue");

    [MenuItem("GameObject/Set Hierarchy Color/Purple", false, -46)]
    static void SetPurple() => SetColor("Purple");

    [MenuItem("GameObject/Set Hierarchy Color/Yellow", false, -45)]
    static void SetYellow() => SetColor("Yellow");

    #endregion

    static void SetColor(string colorName)
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            string key = $"HierarchyColor_{obj.GetInstanceID()}";
            EditorPrefs.SetString(key, colorName);
        }

        EditorApplication.RepaintHierarchyWindow();
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
