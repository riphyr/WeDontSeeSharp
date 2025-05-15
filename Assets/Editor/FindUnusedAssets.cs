using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class FindUnusedAssets : EditorWindow
{
    private List<string> unusedAssets = new List<string>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Find Unused Assets")]
    public static void ShowWindow()
    {
        GetWindow<FindUnusedAssets>("Find Unused Assets");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Scan Unused Assets"))
        {
            ScanUnusedAssets();
        }

        if (unusedAssets.Count > 0)
        {
            GUILayout.Label("Unused Assets Found: " + unusedAssets.Count, EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            foreach (var asset in unusedAssets)
            {
                GUILayout.Label(asset, EditorStyles.wordWrappedLabel);
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Delete All Unused Assets (⚠️ Irreversible)"))
            {
                if (EditorUtility.DisplayDialog("Confirm Deletion",
                    "This will permanently delete all unused assets. Are you sure?", "Delete", "Cancel"))
                {
                    DeleteUnusedAssets();
                }
            }
        }
        else
        {
            GUILayout.Label("No unused assets found.", EditorStyles.boldLabel);
        }
    }

    private void ScanUnusedAssets()
    {
        unusedAssets.Clear();
        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        HashSet<string> usedAssets = new HashSet<string>();

        // Récupérer les dépendances des scènes dans la build
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            string[] dependencies = AssetDatabase.GetDependencies(scene.path, true);
            foreach (var dependency in dependencies)
            {
                usedAssets.Add(dependency);
            }
        }

        // Récupérer les dépendances des Prefabs et des Scripts
        string[] allPrefabs = Directory.GetFiles("Assets/", "*.prefab", SearchOption.AllDirectories);
        foreach (var prefab in allPrefabs)
        {
            string[] dependencies = AssetDatabase.GetDependencies(prefab, true);
            foreach (var dependency in dependencies)
            {
                usedAssets.Add(dependency);
            }
        }

        string[] allScripts = Directory.GetFiles("Assets/", "*.cs", SearchOption.AllDirectories);
        foreach (var script in allScripts)
        {
            usedAssets.Add(script);
        }

        // Détection des assets inutilisés
        foreach (var asset in allAssets)
        {
            if (asset.StartsWith("Assets/") && !usedAssets.Contains(asset))
            {
                // Vérification des dépendances
                string[] dependencies = AssetDatabase.GetDependencies(asset, true);
                bool isUsed = false;

                foreach (var dependency in dependencies)
                {
                    if (usedAssets.Contains(dependency))
                    {
                        isUsed = true;
                        break;
                    }
                }

                if (!isUsed)
                {
                    unusedAssets.Add(asset);
                }
            }
        }

        Debug.Log($"Found {unusedAssets.Count} unused assets.");
    }

    private void DeleteUnusedAssets()
    {
        foreach (var asset in unusedAssets)
        {
            AssetDatabase.DeleteAsset(asset);
        }
        AssetDatabase.Refresh();
        Debug.Log("Unused assets deleted!");
    }
}
