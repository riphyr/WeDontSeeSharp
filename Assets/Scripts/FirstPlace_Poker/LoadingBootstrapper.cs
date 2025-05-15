using UnityEngine;

public class LoadingBootstrapper : MonoBehaviour
{
    [Header("Référence au prefab de LoadingManager")]
    public GameObject loadingManagerPrefab;

    private static bool loadingManagerSpawned = false;

    void Awake()
    {
        if (!loadingManagerSpawned && LoadingManager.Instance == null)
        {
            if (loadingManagerPrefab != null)
            {
                GameObject instance = Instantiate(loadingManagerPrefab);
                DontDestroyOnLoad(instance);
                loadingManagerSpawned = true;
                Debug.Log("[Bootstrapper] LoadingManager instancié via prefab référencé.");
            }
            else
            {
                Debug.LogError("[Bootstrapper] Aucune prefab LoadingManager assignée dans l’inspecteur !");
            }
        }
    }
}

