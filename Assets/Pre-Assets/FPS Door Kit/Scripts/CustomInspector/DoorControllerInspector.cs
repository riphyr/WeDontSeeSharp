
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace EnivStudios.dc
{
    [CustomEditor(typeof(DoorController))]
    public class DoorControllerInspector : Editor
    {
        SerializedProperty AnimationType,InverseDoorRotX,InverseDoorRotY,InverseDoorRotZ;
        SerializedProperty States,CircleTimer;
        SerializedProperty TwoWay;
        SerializedProperty DoorRotationSpeed;
        SerializedProperty AutoClose;
        SerializedProperty AutoCloseTimer;
        SerializedProperty DoorOpenUI;
        SerializedProperty DoorCloseUI;
        SerializedProperty OpenDoorSound;
        SerializedProperty CloseDoorSound;
        SerializedProperty DoorTransform;
        SerializedProperty TargetDoorRotation;
        SerializedProperty LockedDoorUI;
        SerializedProperty LockedDoorSound, UseKeySound;
        SerializedProperty LockedDoorHint;
        SerializedProperty TextDisplayTimer;
        SerializedProperty UnlockKey;
        SerializedProperty UseKeyUI;
        SerializedProperty KeyPrefab, LockModel,DoorAxis;
        SerializedProperty UnlockKeycard, KeycardAnimation, KeyAnimation;
        private void OnEnable()
        {
            AnimationType = serializedObject.FindProperty("animationType");
            States = serializedObject.FindProperty("doorType");
            TwoWay = serializedObject.FindProperty("twoWay");
            DoorRotationSpeed = serializedObject.FindProperty("doorRotationSpeed");
            AutoClose = serializedObject.FindProperty("autoClose");
            AutoCloseTimer = serializedObject.FindProperty("autoCloseTimer");
            DoorOpenUI = serializedObject.FindProperty("openDoorUI");
            DoorCloseUI = serializedObject.FindProperty("closeDoorUI");
            OpenDoorSound = serializedObject.FindProperty("doorOpenSound");
            CloseDoorSound = serializedObject.FindProperty("doorCloseSound");
            DoorTransform = serializedObject.FindProperty("door");
            TargetDoorRotation = serializedObject.FindProperty("targetDoor");
            LockedDoorUI = serializedObject.FindProperty("lockedDoorUI");
            LockedDoorSound = serializedObject.FindProperty("doorLockSound");
            LockedDoorHint = serializedObject.FindProperty("doorLockedText");
            TextDisplayTimer = serializedObject.FindProperty("textDisplayTimer");
            UseKeySound = serializedObject.FindProperty("useKeySound");
            UnlockKey = serializedObject.FindProperty("unlockKey");
            UseKeyUI = serializedObject.FindProperty("useKeyUI");
            KeyPrefab = serializedObject.FindProperty("keyPrefab");
            LockModel = serializedObject.FindProperty("lockModel");
            UnlockKeycard = serializedObject.FindProperty("unlockKeycard");
            KeycardAnimation = serializedObject.FindProperty("keycardAnimation");
            KeyAnimation = serializedObject.FindProperty("keyAnim");
            DoorAxis = serializedObject.FindProperty("doorForwardAxis");
            InverseDoorRotX = serializedObject.FindProperty("inverseDoorRotationX");
            InverseDoorRotY = serializedObject.FindProperty("inverseDoorRotationY");
            InverseDoorRotZ = serializedObject.FindProperty("inverseDoorRotationZ");
            CircleTimer = serializedObject.FindProperty("circleTimerr");
        }
        public override void OnInspectorGUI()
        {
            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;

            EditorGUILayout.LabelField("Door Controller Script", Header);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("This script handles all the door types and is fully dynamic in inspector.", GuiMessageStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(States);
            EditorGUILayout.Space();

            var stateType = (DoorController.states)States.enumValueIndex;
            var animType = (DoorController.anims)AnimationType.enumValueIndex;
            var doorAxis = (DoorController.DoorAxis)DoorAxis.enumValueIndex;

            switch (stateType)
            {
                case DoorController.states.NormalDoor:
                    GUILayout.Label("Door Properties", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(TwoWay);
                    if (!TwoWay.boolValue)
                    {
                        EditorGUILayout.PropertyField(TargetDoorRotation, new GUIContent("Target Door RotationY"));
                    }
                    else if (TwoWay.boolValue)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(DoorAxis);
                        switch (doorAxis)
                        {
                            case DoorController.DoorAxis.XAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotX);
                                break;
                            case DoorController.DoorAxis.YAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotY);
                                break;
                            case DoorController.DoorAxis.ZAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotZ);
                                break;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(DoorRotationSpeed, 1, 10, new GUIContent("Door Rotation Speed"));
                    EditorGUILayout.PropertyField(AutoClose);
                    if (AutoClose.boolValue)
                    {
                        EditorGUILayout.PropertyField(AutoCloseTimer);
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorOpenUI);
                    EditorGUILayout.PropertyField(DoorCloseUI);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OpenDoorSound);
                    EditorGUILayout.PropertyField(CloseDoorSound);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Transform", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorTransform, new GUIContent("Normal Door Pivot"));
                    break;

                case DoorController.states.HoldKeyDoor:
                    GUILayout.Label("Door Properties", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(TwoWay);
                    if (!TwoWay.boolValue)
                    {
                        EditorGUILayout.PropertyField(TargetDoorRotation, new GUIContent("Target Door RotationY"));
                    }
                    else if (TwoWay.boolValue)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(DoorAxis);
                        switch (doorAxis)
                        {
                            case DoorController.DoorAxis.XAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotX);
                                break;
                            case DoorController.DoorAxis.YAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotY);
                                break;
                            case DoorController.DoorAxis.ZAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotZ);
                                break;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(DoorRotationSpeed, 1, 10, new GUIContent("Door Rotation Speed"));
                    EditorGUILayout.Slider(CircleTimer, 0.5f, 4.5f,new GUIContent("Circle Timer"));
                    EditorGUILayout.PropertyField(AutoClose);
                    if (AutoClose.boolValue)
                    {
                        EditorGUILayout.PropertyField(AutoCloseTimer);
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorOpenUI);
                    EditorGUILayout.PropertyField(DoorCloseUI);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OpenDoorSound);
                    EditorGUILayout.PropertyField(CloseDoorSound);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Transform", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorTransform, new GUIContent("HoldKey Door Pivot"));
                    break;

                case DoorController.states.LeverDoor:
                    GUILayout.Label("Door Properties", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(TwoWay);
                    if (!TwoWay.boolValue)
                    {
                        EditorGUILayout.PropertyField(TargetDoorRotation, new GUIContent("Target Door RotationY"));
                    }
                    else if (TwoWay.boolValue)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(DoorAxis);
                        switch (doorAxis)
                        {
                            case DoorController.DoorAxis.XAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotX);
                                break;
                            case DoorController.DoorAxis.YAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotY);
                                break;
                            case DoorController.DoorAxis.ZAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotZ);
                                break;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(DoorRotationSpeed, 1, 10, new GUIContent("Door Rotation Speed"));
                    EditorGUILayout.PropertyField(AutoClose);
                    if (AutoClose.boolValue)
                    {
                        EditorGUILayout.PropertyField(AutoCloseTimer);
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(LockedDoorUI);
                    EditorStyles.textField.wordWrap = true;
                    EditorGUILayout.PropertyField(LockedDoorHint, new GUIContent("Locked Door Hint"), GUILayout.Height(33));
                    EditorGUILayout.PropertyField(TextDisplayTimer);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(DoorOpenUI);
                    EditorGUILayout.PropertyField(DoorCloseUI);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OpenDoorSound);
                    EditorGUILayout.PropertyField(CloseDoorSound);
                    EditorGUILayout.PropertyField(LockedDoorSound);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Transform", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorTransform, new GUIContent("Lever Door Pivot"));
                    break;

                case DoorController.states.KeyDoor:
                    EditorGUILayout.Space(-8);
                    EditorGUILayout.PropertyField(AnimationType);
                    switch (animType)
                    {
                        case DoorController.anims.NoAnimation:
                            EditorGUILayout.PropertyField(UnlockKey);
                            break;
                        case DoorController.anims.LockKeyAnimation:
                            EditorGUILayout.PropertyField(UnlockKey);
                            EditorGUILayout.PropertyField(LockModel);
                            break;
                        case DoorController.anims.KeyAnimation:
                            EditorGUILayout.PropertyField(UnlockKey);
                            EditorGUILayout.PropertyField(KeyPrefab, new GUIContent("Key Holder"));
                            break;
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Properties", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(TwoWay);
                    if (!TwoWay.boolValue)
                    {
                        EditorGUILayout.PropertyField(TargetDoorRotation, new GUIContent("Target Door RotationY"));
                    }
                    else if (TwoWay.boolValue)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(DoorAxis);
                        switch (doorAxis)
                        {
                            case DoorController.DoorAxis.XAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotX);
                                break;
                            case DoorController.DoorAxis.YAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotY);
                                break;
                            case DoorController.DoorAxis.ZAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotZ);
                                break;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(DoorRotationSpeed, 1, 10, new GUIContent("Door Rotation Speed"));
                    EditorGUILayout.PropertyField(AutoClose);
                    if (AutoClose.boolValue)
                    {
                        EditorGUILayout.PropertyField(AutoCloseTimer);
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(LockedDoorUI);
                    EditorStyles.textField.wordWrap = true;
                    EditorGUILayout.PropertyField(LockedDoorHint, new GUIContent("Locked Door Hint"), GUILayout.Height(33));
                    EditorGUILayout.PropertyField(TextDisplayTimer);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(DoorOpenUI);
                    EditorGUILayout.PropertyField(DoorCloseUI);
                    switch (animType)
                    {
                        case DoorController.anims.NoAnimation:
                            EditorGUILayout.PropertyField(UseKeyUI);
                            break;
                        case DoorController.anims.KeyAnimation:
                            EditorGUILayout.PropertyField(UseKeyUI);
                            EditorGUILayout.Space();
                            GUILayout.Label("Key Animation", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(KeyAnimation);
                            break;
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OpenDoorSound);
                    EditorGUILayout.PropertyField(CloseDoorSound);
                    EditorGUILayout.PropertyField(LockedDoorSound);
                    switch (animType)
                    {
                        case DoorController.anims.NoAnimation:
                            EditorGUILayout.PropertyField(UseKeySound);
                            break;
                        case DoorController.anims.KeyAnimation:
                            EditorGUILayout.PropertyField(UseKeySound);
                            break;
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Transform", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorTransform, new GUIContent("Key Door Pivot"));
                    break;

                case DoorController.states.KeypadDoor:
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Properties", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(TwoWay);
                    if (!TwoWay.boolValue)
                    {
                        EditorGUILayout.PropertyField(TargetDoorRotation, new GUIContent("Target Door RotationY"));
                    }
                    else if (TwoWay.boolValue)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(DoorAxis);
                        switch (doorAxis)
                        {
                            case DoorController.DoorAxis.XAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotX);
                                break;
                            case DoorController.DoorAxis.YAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotY);
                                break;
                            case DoorController.DoorAxis.ZAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotZ);
                                break;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(DoorRotationSpeed, 1, 10, new GUIContent("Door Rotation Speed"));
                    EditorGUILayout.PropertyField(AutoClose);
                    if (AutoClose.boolValue)
                    {
                        EditorGUILayout.PropertyField(AutoCloseTimer);
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(LockedDoorUI);
                    EditorStyles.textField.wordWrap = true;
                    EditorGUILayout.PropertyField(LockedDoorHint, new GUIContent("Locked Door Hint"), GUILayout.Height(33));
                    EditorGUILayout.PropertyField(TextDisplayTimer);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(DoorOpenUI);
                    EditorGUILayout.PropertyField(DoorCloseUI);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OpenDoorSound);
                    EditorGUILayout.PropertyField(CloseDoorSound);
                    EditorGUILayout.PropertyField(LockedDoorSound);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Transform", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorTransform, new GUIContent("Keypad Door Pivot"));
                    break;

                case DoorController.states.KeycardDoor:
                    EditorGUILayout.Space(-8);
                    EditorGUILayout.PropertyField(UnlockKeycard);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Properties", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(KeycardAnimation);
                    EditorGUILayout.PropertyField(TwoWay);
                    if (!TwoWay.boolValue)
                    {
                        EditorGUILayout.PropertyField(TargetDoorRotation, new GUIContent("Target Door RotationY"));
                    }
                    else if (TwoWay.boolValue)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(DoorAxis);
                        switch (doorAxis)
                        {
                            case DoorController.DoorAxis.XAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotX);
                                break;
                            case DoorController.DoorAxis.YAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotY);
                                break;
                            case DoorController.DoorAxis.ZAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotZ);
                                break;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(DoorRotationSpeed, 1, 10, new GUIContent("Door Rotation Speed"));
                    EditorGUILayout.PropertyField(AutoClose);
                    if (AutoClose.boolValue)
                    {
                        EditorGUILayout.PropertyField(AutoCloseTimer);
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(LockedDoorUI);
                    EditorStyles.textField.wordWrap = true;
                    EditorGUILayout.PropertyField(LockedDoorHint, new GUIContent("Locked Door Hint"), GUILayout.Height(33));
                    EditorGUILayout.PropertyField(TextDisplayTimer);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(DoorOpenUI);
                    EditorGUILayout.PropertyField(DoorCloseUI);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OpenDoorSound);
                    EditorGUILayout.PropertyField(CloseDoorSound);
                    EditorGUILayout.PropertyField(LockedDoorSound);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Transform", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorTransform, new GUIContent("Keycard Door Pivot"));
                    break;

                case DoorController.states.PadLockDoor:
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Properties", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(TwoWay);
                    if (!TwoWay.boolValue)
                    {
                        EditorGUILayout.PropertyField(TargetDoorRotation, new GUIContent("Target Door RotationY"));
                    }
                    else if (TwoWay.boolValue)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(DoorAxis);
                        switch (doorAxis)
                        {
                            case DoorController.DoorAxis.XAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotX);
                                break;
                            case DoorController.DoorAxis.YAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotY);
                                break;
                            case DoorController.DoorAxis.ZAxis:
                                EditorGUILayout.PropertyField(InverseDoorRotZ);
                                break;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Slider(DoorRotationSpeed, 1, 10, new GUIContent("Door Rotation Speed"));
                    EditorGUILayout.PropertyField(AutoClose);
                    if (AutoClose.boolValue)
                    {
                        EditorGUILayout.PropertyField(AutoCloseTimer);
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label("Door UI", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(LockedDoorUI);
                    EditorStyles.textField.wordWrap = true;
                    EditorGUILayout.PropertyField(LockedDoorHint, new GUIContent("Locked Door Hint"), GUILayout.Height(33));
                    EditorGUILayout.PropertyField(TextDisplayTimer);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(DoorOpenUI);
                    EditorGUILayout.PropertyField(DoorCloseUI);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Sounds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OpenDoorSound);
                    EditorGUILayout.PropertyField(CloseDoorSound);
                    EditorGUILayout.PropertyField(LockedDoorSound);
                    EditorGUILayout.Space();
                    GUILayout.Label("Door Transform", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(DoorTransform, new GUIContent("Padlock Door Pivot"));
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