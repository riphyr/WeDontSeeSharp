
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace EnivStudios.lc
{
    [CustomEditor(typeof(LockController))]

    public class LockControllerInspector : Editor
    {
        SerializedProperty Locks, KeycardText, KeyLockAnim, UseKeySound, InsertSound;
        SerializedProperty Keycard, LockOpenAnim, WrongCodeSound, RightCodeSound;
        SerializedProperty BlueKeycardAnim, KeycardAnimationType;
        SerializedProperty AntiSpamTime, RotateSound;
        SerializedProperty ItemRotation, ExamineLockedText, Keypad;
        SerializedProperty NormalLockType, UnlockCode, Combination, PadLockOpen, LockpickingSound;
        private void OnEnable()
        {
            Locks = serializedObject.FindProperty("lockType");
            ItemRotation = serializedObject.FindProperty("itemRotation");
            ExamineLockedText = serializedObject.FindProperty("examineLockedText");
            NormalLockType = serializedObject.FindProperty("normallockType");
            UnlockCode = serializedObject.FindProperty("keypadNumber");
            Combination = serializedObject.FindProperty("combination");
            BlueKeycardAnim = serializedObject.FindProperty("blueKeycardAnim");
            Keycard = serializedObject.FindProperty("keycard");
            KeycardText = serializedObject.FindProperty("keycardText");
            KeyLockAnim = serializedObject.FindProperty("keyLockAnim");
            LockOpenAnim = serializedObject.FindProperty("lockOpenAnim");
            UseKeySound = serializedObject.FindProperty("useKeySound");
            AntiSpamTime = serializedObject.FindProperty("antiSpamTime");
            WrongCodeSound = serializedObject.FindProperty("wrongCodeSound");
            Keypad = serializedObject.FindProperty("keypad");
            RightCodeSound = serializedObject.FindProperty("rightCodeSound");
            PadLockOpen = serializedObject.FindProperty("padLockOpen");
            RotateSound = serializedObject.FindProperty("rotateSound");
            InsertSound = serializedObject.FindProperty("insertSound");
            KeycardAnimationType = serializedObject.FindProperty("keycardAnimationType");
            LockpickingSound = serializedObject.FindProperty("lockpickingSound");
        }
        public override void OnInspectorGUI()
        {
            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;

            EditorGUILayout.LabelField("Lock Controller Script", Header);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("This script handles all the lock types and is fully dynamic in inspector.", GuiMessageStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(Locks);

            var lockTypes = (LockController.locks)Locks.enumValueIndex;
            var animType = (LockController.anims)KeycardAnimationType.enumValueIndex;

            switch (lockTypes)
            {

                case LockController.locks.NormalLock:
                    EditorGUILayout.PropertyField(NormalLockType, new GUIContent("Normal Lock Type"));
                    EditorGUILayout.Space();
                    GUILayout.Label("Object Rotation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(ItemRotation, new GUIContent("Object Rotation"));
                    EditorGUILayout.Space(); EditorGUILayout.Space();
                    EditorGUILayout.Slider(AntiSpamTime, 0.5f, 2);
                    EditorGUILayout.Space();
                    GUILayout.Label("Unlock Sound", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(UseKeySound);
                    EditorGUILayout.Space();
                    GUILayout.Label("Lock Animations", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(KeyLockAnim);
                    EditorGUILayout.PropertyField(LockOpenAnim);
                    EditorGUILayout.Space();
                    GUILayout.Label("Lock UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(ExamineLockedText);
                    break;

                case LockController.locks.Keypad:
                    EditorGUILayout.Space();
                    GUILayout.Label("Object Rotation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(ItemRotation, new GUIContent("Object Rotation"));
                    EditorGUILayout.Space(); EditorGUILayout.Space();
                    EditorGUILayout.Slider(AntiSpamTime, 0.5f, 2);
                    EditorGUILayout.Space();
                    GUILayout.Label("Unlock Code", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(UnlockCode, new GUIContent("Unlock Code"));
                    EditorGUILayout.Space();
                    GUILayout.Label("Keypad UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(Keypad, new GUIContent("Keypad Text"));
                    EditorGUILayout.Space();
                    GUILayout.Label("Keypad Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(WrongCodeSound);
                    EditorGUILayout.PropertyField(RightCodeSound, new GUIContent("Correct Code Sound"));
                    break;

                case LockController.locks.Padlock:
                    EditorGUILayout.Space();
                    GUILayout.Label("Object Rotation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(ItemRotation, new GUIContent("Object Rotation"));
                    EditorGUILayout.Space(); EditorGUILayout.Space();
                    EditorGUILayout.Slider(AntiSpamTime, 0.5f, 2);
                    EditorGUILayout.Space();
                    GUILayout.Label("Unlock Code", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(Combination, new GUIContent("Unlock Code"));
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Unlock Code should be of 4 digits only.", MessageType.Info);
                    EditorGUILayout.Space();
                    GUILayout.Label("Padlock Animation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(PadLockOpen);
                    EditorGUILayout.Space();
                    GUILayout.Label("Padlock Sound", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(RotateSound);
                    break;

                case LockController.locks.CardReader:
                    EditorGUILayout.PropertyField(Keycard, new GUIContent("Card Reader Type"));
                    EditorGUILayout.PropertyField(KeycardAnimationType, new GUIContent("Animation Type"));
                    EditorGUILayout.Space();
                    switch (animType)
                    {
                        case LockController.anims.KeycardAnimation:
                            GUILayout.Label("Object Rotation", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(ItemRotation, new GUIContent("Object Rotation"));
                            EditorGUILayout.Space(); EditorGUILayout.Space();
                            GUILayout.Label("Keycard Animation", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(BlueKeycardAnim, new GUIContent("Keycard Anim"));
                            break;
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(AntiSpamTime, 0.5f, 2);
                    EditorGUILayout.Space();
                    GUILayout.Label("Keycard UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(KeycardText);
                    EditorGUILayout.Space();
                    GUILayout.Label("CardReader Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(InsertSound);
                    EditorGUILayout.PropertyField(WrongCodeSound, new GUIContent("Wrong Card Sound"));
                    EditorGUILayout.PropertyField(RightCodeSound, new GUIContent("Right Card Sound"));
                    break;
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
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