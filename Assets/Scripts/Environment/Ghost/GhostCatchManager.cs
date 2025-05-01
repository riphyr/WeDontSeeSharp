using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class GhostCatchManager : MonoBehaviour
{
    public static GhostCatchManager Instance { get; private set; }

    [Header("General Settings")]
    public int maxLives = 3;
    private int currentLives;

    [Header("Solo Settings")]
    public Transform soloRespawnPoint;

    [Header("Multiplayer Settings")]
    public Transform[] chainPoints;
    public InteractionScripts.CaptureLock[] locks;
    public GameObject keyPrefab;
    public Transform keySpawnPoint;

    [Header("IA Scaling After 0 Lives (Multiplayer)")]
    public GhostAI ghostAI;
    public float visionBonus = 5f;
    public float patrolSpeedMultiplier = 1.2f;
    public float chaseSpeedMultiplier = 1.25f;

    private List<GameObject> chainedPlayers = new List<GameObject>();
    private bool keySpawned = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentLives = maxLives;
    }

    public void OnPlayerCaught(GameObject player)
    {
        if (GameManager.Instance.IsSoloMode())
            HandleSoloCapture(player);
        else
            HandleMultiplayerCapture(player);
    }

    #region SOLO
    private void HandleSoloCapture(GameObject player)
    {
        currentLives--;
        Debug.Log($"[GhostCatch] SOLO player caught. Lives left: {currentLives}");

        if (currentLives <= 0)
        {
            TriggerGameOver(player);
            return;
        }

        // Respawn
        CharacterController ctrl = player.GetComponent<CharacterController>();
        if (ctrl != null) ctrl.enabled = false;

        player.transform.position = soloRespawnPoint.position;

        if (ctrl != null) ctrl.enabled = true;

        // TODO: animation, voix, fondu
    }
    #endregion

    #region MULTI
    private void HandleMultiplayerCapture(GameObject player)
    {
        if (chainedPlayers.Contains(player)) return;

        currentLives--;
        chainedPlayers.Add(player);
        Debug.Log($"[GhostCatch] MULTI: {player.name} enchaîné. Lives restantes : {currentLives}");

        // Immobiliser & TP
        CharacterController ctrl = player.GetComponent<CharacterController>();
        if (ctrl != null) ctrl.enabled = false;

        int idx = Mathf.Min(chainedPlayers.Count - 1, chainPoints.Length - 1);
        player.transform.position = chainPoints[idx].position;

        if (idx < locks.Length)
        {
            locks[idx].playerToUnchain = player;
            locks[idx].targetToFree.SetActive(true); // optionnel : montrer la chaîne
        }

        // Spawn key
        if (!keySpawned)
        {
            PhotonNetwork.Instantiate(keyPrefab.name, keySpawnPoint.position, Quaternion.identity);
            keySpawned = true;
        }

        if (currentLives <= 0)
        {
            ApplyGhostDifficultyBoost();
        }

        CheckIfAllPlayersChained();
    }

    private void ApplyGhostDifficultyBoost()
    {
        ghostAI.viewDistance += visionBonus;
        ghostAI.patrolSpeed *= patrolSpeedMultiplier;
        ghostAI.chaseSpeed *= chaseSpeedMultiplier;

        Debug.Log("[GhostCatch] L'IA est boostée (0 vie restante)");
    }

    private void CheckIfAllPlayersChained()
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag("Player");

        bool allCaught = true;
        foreach (GameObject p in all)
        {
            if (!chainedPlayers.Contains(p))
            {
                allCaught = false;
                break;
            }
        }

        if (allCaught)
        {
            Debug.Log("[GhostCatch] Tous les joueurs sont enchaînés. Game Over.");
            TriggerGameOver(null);
        }
    }
    #endregion

    private void TriggerGameOver(GameObject player)
    {
        Debug.Log("[GhostCatch] GAME OVER déclenché.");

        // TODO: animation, écran noir, retour menu, reset save JSON etc.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameData();
        }

        PhotonNetwork.LoadLevel("HouseLvl"); // ou scène de fin
    }

    public void UnchainPlayer(GameObject player)
    {
        if (chainedPlayers.Contains(player))
        {
            chainedPlayers.Remove(player);
            CharacterController ctrl = player.GetComponent<CharacterController>();
            if (ctrl != null) ctrl.enabled = true;

            // TODO: re-activer UI / gameplay
        }
    }
}
