using UnityEditor;
using UnityEngine;

public class TextureOptimizer
{
    [MenuItem("Tools/Optimise Textures/Compress & Resize Textures (Optimized)")]
    public static void OptimiseTextures()
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

                    // Compression settings
                    if (importer.textureCompression != TextureImporterCompression.Compressed ||
                        !importer.crunchedCompression || importer.compressionQuality < 50)
                    {
                        importer.textureCompression = TextureImporterCompression.Compressed;
                        importer.crunchedCompression = true;
                        importer.compressionQuality = 50;
                        changed = true;
                    }

                    // Max size limit
                    if (importer.maxTextureSize > 2048)
                    {
                        importer.maxTextureSize = 2048;
                        changed = true;
                    }

                    // Enable mipmaps
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

                if (EditorUtility.DisplayCancelableProgressBar("Compressing & Resizing Textures",
                    $"Processing texture {i + 1}/{guids.Length}", i / (float)guids.Length))
                {
                    Debug.LogWarning("⚠️ Operation cancelled by user.");
                    break;
                }
            }

            Debug.Log($"✅ Textures optimisées : {processed}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
