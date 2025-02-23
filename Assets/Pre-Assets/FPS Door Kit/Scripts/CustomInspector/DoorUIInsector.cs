
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace EnivStudios.di
{
    [CustomEditor(typeof(DoorUI))]
    public class DoorUIInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;


            EditorGUILayout.LabelField("Door UI Script", header);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("This script handles door UI elements.", guiMessageStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawDefaultInspector();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

        }
        GUIStyle guiMessageStyle
        {
            get
            {
                var messageStyle = new GUIStyle(GUI.skin.label);
                messageStyle.wordWrap = true;
                return messageStyle;
            }
        }
        GUIStyle header
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
