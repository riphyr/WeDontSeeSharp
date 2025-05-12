using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public static class HierarchyColorizerLayered
{
    static Texture2D iconGray;
    static Texture2D iconBlue;
    static Texture2D iconRed;
    
    static Texture2D iconVisible;
    static Texture2D iconPartialHiddenA;
    static Texture2D iconPartialHiddenB;
    static Texture2D iconFullyHidden;
    
    static Texture2D iconPickable;
    static Texture2D iconPartialPickableA;
    static Texture2D iconPartialPickableB;
    static Texture2D iconFullyUnpickable;
    
    static HierarchyColorizerLayered()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

        iconGray = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
        iconBlue = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
        iconRed = EditorGUIUtility.IconContent("console.erroricon.sml").image as Texture2D;
        
        iconVisible = EditorGUIUtility.Load("Icons/eye_visible.png") as Texture2D;
        iconPartialHiddenA = EditorGUIUtility.Load("Icons/eye_partialhidden_a.png") as Texture2D;
        iconPartialHiddenB = EditorGUIUtility.Load("Icons/eye_partialhidden_b.png") as Texture2D;
        iconFullyHidden = EditorGUIUtility.Load("Icons/eye_fullyhidden.png") as Texture2D;
        
        iconPickable = EditorGUIUtility.Load("Icons/pickable.png") as Texture2D;
        iconPartialPickableA = EditorGUIUtility.Load("Icons/children.png") as Texture2D;
        iconPartialPickableB = EditorGUIUtility.Load("Icons/children.png") as Texture2D;
        iconFullyUnpickable = EditorGUIUtility.Load("Icons/unpickable.png") as Texture2D;
    }

    static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;
        
        if (Event.current.type != EventType.Repaint)
            return;

        string colorName = HierarchyColorDatabase.Instance.GetColor(obj);

        Rect fullRect = new Rect(0, selectionRect.y, EditorGUIUtility.currentViewWidth, selectionRect.height);
        
        bool isSelected = Selection.instanceIDs.Contains(instanceID);
        bool isHovered = fullRect.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint)
        {
            float borderWidth = 32f;

            Rect leftBorderRect = new Rect(0, selectionRect.y, borderWidth, selectionRect.height);
            EditorGUI.DrawRect(leftBorderRect, new Color(0.19f, 0.19f, 0.19f, 1f));

            Rect mainRect = new Rect(borderWidth, selectionRect.y, EditorGUIUtility.currentViewWidth - borderWidth, selectionRect.height);
            EditorGUI.DrawRect(mainRect, new Color(0.22f, 0.22f, 0.22f, 1f));

            if (isSelected)
                EditorGUI.DrawRect(fullRect, new Color(0.173f, 0.365f, 0.529f, 1f));
            else if (isHovered)
                EditorGUI.DrawRect(fullRect, new Color(1f, 1f, 1f, 0.06f));
        }

        if (obj.transform.childCount > 0)
        {
            float baseOffset = 14f;
            float indentLevel = (selectionRect.x - 16f) / 14f;
            float trueX = indentLevel * 14f;

            Rect triangleRect = new Rect(trueX, selectionRect.y, 16f, 16f);

            if (Event.current.type == EventType.Repaint)
            {
                bool isExpanded = IsExpanded(instanceID);

                Matrix4x4 oldMatrix = GUI.matrix;

                if (isExpanded)
                {
                    Vector2 pivotPoint = new Vector2(triangleRect.x + triangleRect.width * 0.5f, triangleRect.y + triangleRect.height * 0.5f);
                    GUIUtility.RotateAroundPivot(360, pivotPoint);
                    triangleRect.x += 0.6f;
                }

                EditorStyles.foldout.Draw(triangleRect, GUIContent.none, false, false, isExpanded, false);
                GUI.matrix = oldMatrix;
            }
        }

        if (Event.current.type == EventType.Repaint)
        {
            const float iconSize = 16f;
            Rect iconRect = new Rect(0f, selectionRect.y + (selectionRect.height - iconSize) / 2f, iconSize, iconSize);
            Texture2D iconToDraw = GetVisibilityIcon(obj, isHovered);
            if (iconToDraw != null)
                GUI.DrawTexture(iconRect, iconToDraw, ScaleMode.ScaleToFit, true);
        }
        
        if (Event.current.type == EventType.Repaint)
        {
            const float iconSize = 16f;
            Rect iconRectPick = new Rect(15f, selectionRect.y + (selectionRect.height - iconSize) / 2f, iconSize, iconSize);
            Texture2D iconPick = GetPickabilityIcon(obj, isHovered);
            if (iconPick != null)
                GUI.DrawTexture(iconRectPick, iconPick, ScaleMode.ScaleToFit, true);
        }

        Texture2D icon = GetIconForObject(obj);
        Rect iconPrefabRect = new Rect(selectionRect.x + 20f, selectionRect.y, 16f, 16f);
        if (icon != null)
            GUI.DrawTexture(iconPrefabRect, icon);

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

        Rect labelRect = new Rect(selectionRect.x + 40f, selectionRect.y - 0.6f, selectionRect.width - 40f, selectionRect.height);
        EditorGUI.LabelField(labelRect, obj.name, labelStyle);
    }

    static bool IsExpanded(int instanceID)
    {
        var hierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        if (hierarchyWindowType == null) return false;

        var lastInteractedProp = hierarchyWindowType.GetProperty("lastInteractedHierarchyWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (lastInteractedProp == null) return false;

        var lastWindow = lastInteractedProp.GetValue(null);
        if (lastWindow == null) return false;

        var getExpandedIDs = hierarchyWindowType.GetMethod("GetExpandedIDs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (getExpandedIDs == null) return false;

        var expandedIDs = getExpandedIDs.Invoke(lastWindow, null) as int[];
        if (expandedIDs == null) return false;

        return expandedIDs.Contains(instanceID);
    }

    static Texture2D GetIconForObject(GameObject obj)
    {
        var status = PrefabUtility.GetPrefabInstanceStatus(obj);
        if (status == PrefabInstanceStatus.MissingAsset)
            return iconRed;
        else if (status == PrefabInstanceStatus.Connected)
            return iconBlue;
        else
            return iconGray;
    }
    
    static Texture2D GetVisibilityIcon(GameObject obj, bool isHovered)
    {
        bool isHidden = SceneVisibilityUtility.IsHidden(obj);
        bool hasHiddenChildren = obj.transform.Cast<Transform>().Any(c => SceneVisibilityUtility.IsHidden(c.gameObject));

        if (isHovered && !isHidden && !hasHiddenChildren) return iconVisible;
        if (!isHidden && hasHiddenChildren) return iconPartialHiddenA;
        if (isHidden && !hasHiddenChildren) return iconFullyHidden;
        if (isHidden && hasHiddenChildren) return iconPartialHiddenB;
        return null;
    }
    
    static Texture2D GetPickabilityIcon(GameObject obj, bool isHovered)
    {
        bool isPickable = SceneVisibilityUtility.IsPickable(obj);
        bool hasUnpickableChildren = obj.transform.Cast<Transform>().Any(c => !SceneVisibilityUtility.IsPickable(c.gameObject));

        if (isHovered && isPickable && !hasUnpickableChildren) return iconPickable;
        if (isPickable && hasUnpickableChildren) return iconPartialPickableA;
        if (!isPickable && !hasUnpickableChildren) return iconFullyUnpickable;
        if (!isPickable && hasUnpickableChildren) return iconPartialPickableB;

        return null;
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

static class SceneVisibilityUtility
{
    static UnityEditor.SceneVisibilityManager manager = UnityEditor.SceneVisibilityManager.instance;

    public static bool IsHidden(GameObject go) => manager.IsHidden(go);

    public static bool IsPickable(GameObject go) => !manager.IsPickingDisabled(go);
}