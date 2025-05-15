using UnityEditor;
using UnityEngine;

public static class HierarchyColorContextMenu
{
    [MenuItem("GameObject/Set Color/Red", false, -99)]
    static void SetColorRed()
    {
        SetColor("Red");
    }

    [MenuItem("GameObject/Set Color/Green", false, -98)]
    static void SetColorGreen()
    {
        SetColor("Green");
    }

    [MenuItem("GameObject/Set Color/Blue", false, -97)]
    static void SetColorBlue()
    {
        SetColor("Blue");
    }

    [MenuItem("GameObject/Set Color/Purple", false, -96)]
    static void SetColorPurple()
    {
        SetColor("Purple");
    }

    [MenuItem("GameObject/Set Color/Yellow", false, -95)]
    static void SetColorYellow()
    {
        SetColor("Yellow");
    }
    
    [MenuItem("GameObject/Set Color/Magenta", false, -94)]
    static void SetColorMagenta()
    {
        SetColor("Magenta");
    }

    [MenuItem("GameObject/Set Color/None", false, -93)]
    static void SetColorNone()
    {
        SetColor("None");
    }

    static void SetColor(string color)
    {
        foreach (var obj in Selection.gameObjects)
        {
            HierarchyColorDatabase.Instance.SetColor(obj, color);
        }

        EditorApplication.RepaintHierarchyWindow();
    }
}