using UnityEngine;
using System.IO;

[System.Serializable]
public class GameSaveData
{
    public bool isMultiplayer;
    public string roomName;
    public string currentScene;
    public float playTime;
}

public static class GameSaveManager
{
    private static string savePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Save written at: " + savePath);
    }

    public static GameSaveData Load()
    {
        if (isSaveAvailable())
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        else
        {
            Debug.LogWarning("No save file found.");
            return null;
        }
    }

    public static bool isSaveAvailable()
    {
        return File.Exists(savePath);
    }
}