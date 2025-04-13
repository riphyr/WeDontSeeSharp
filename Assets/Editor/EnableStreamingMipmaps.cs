using UnityEditor;
using UnityEngine;

public class EnableStreamingMipmaps
{
    [MenuItem("Tools/Optimise Textures/Enable Streaming Mipmaps (Optimized)")]
    public static void EnableStreaming()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int processed = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null && !importer.assetPath.Contains("Editor"))
                {
                    bool changed = false;

                    if (!importer.streamingMipmaps)
                    {
                        importer.streamingMipmaps = true;
                        changed = true;
                    }

                    if (!importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = true;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                        processed++;
                    }
                }

                if (EditorUtility.DisplayCancelableProgressBar("Enable Streaming Mipmaps",
                        $"Processing texture {i + 1}/{guids.Length}", i / (float)guids.Length))
                {
                    Debug.LogWarning("⚠️ Operation cancelled by user.");
                    break;
                }
            }

            Debug.Log($"✅ Fini ! Textures modifiées : {processed}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}