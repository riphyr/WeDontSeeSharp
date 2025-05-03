using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameSaveData CurrentGameData { get; private set; }

    void Awake()
    {
        Instance = this;

        LoadSaveOnce();
    }

    private void LoadSaveOnce()
    {
        CurrentGameData = GameSaveManager.Load();

        if (CurrentGameData == null)
        {
            Debug.LogWarning("Aucune sauvegarde trouvée !");
        }
        else
        {
            Debug.Log($"Save chargée — Mode multijoueur ? {CurrentGameData.isMultiplayer}");
        }
    }
    
    public void ResetGameData()
    {
        Debug.Log("[GameManager] Reset de la sauvegarde en cours...");
    
        float currentPlayTime = GetPlayTime();
        LoadSaveOnce();
        CurrentGameData.playTime = currentPlayTime;
    }

    public bool IsSoloMode() => CurrentGameData != null && !CurrentGameData.isMultiplayer;
    public bool IsMultiplayerMode() => CurrentGameData != null && CurrentGameData.isMultiplayer;
    public float GetPlayTime() => CurrentGameData.playTime;
}