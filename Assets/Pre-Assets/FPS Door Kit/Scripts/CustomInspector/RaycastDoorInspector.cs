
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace EnivStudios.rd
{
    [CustomEditor(typeof(RaycastDoor))]
    public class RaycastDoorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;

            EditorGUILayout.LabelField("Raycast Door Script", Header);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("This script detect doors and their locks in the game view.", GuiMessageStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawDefaultInspector();

            EditorGUILayout.EndVertical();
        }
        GUIStyle GuiMessageStyle
        {
            get
            {
                var messageStyle = new GUIStyle(GUI.skin.label);
                messageStyle.wordWrap = true;
                return messageStyle;
            }
        }
        GUIStyle Header
        {
            get
            {
                var messageStyle = new GUIStyle(GUI.skin.label);
                messageStyle.wordWrap = true;
                messageStyle.fontStyle = FontStyle.BoldAndItalic;
                messageStyle.fontSize = 18;
                return messageStyle;
            }
        }
    }
}
#endif