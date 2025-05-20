using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LoreDatabase))]
public class LoreDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LoreDatabase db = (LoreDatabase)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Outils de test", EditorStyles.boldLabel);

        foreach (var entry in db.loreEntries)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(entry.itemName);

            string buttonLabel = entry.isDiscovered ? "Cacher" : "Révéler";
            if (GUILayout.Button(buttonLabel, GUILayout.Width(80)))
            {
                entry.isDiscovered = !entry.isDiscovered;
                MarkDirty(db);
                RefreshUIButtons();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void MarkDirty(LoreDatabase db)
    {
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
    }

    private void RefreshUIButtons()
    {
        ListButtonGenerator generator = GameObject.FindObjectOfType<ListButtonGenerator>();
        if (generator != null)
        {
            generator.RefreshButtons();
        }
    }
}