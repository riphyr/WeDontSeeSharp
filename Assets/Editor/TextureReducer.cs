using UnityEditor;
using UnityEngine;

public class TextureReducer
{
    [MenuItem("Tools/Reduce Textures to 2048")]
    public static void ReduceTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && importer.maxTextureSize > 2048)
            {
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
        }
    }
}