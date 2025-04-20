using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class RedLightGreenLightGame : MonoBehaviourPunCallbacks
{
    [Header("Game Settings")]
    public GameObject gameMasterPrefab; // Préfab du meneur de jeu
    public List<Transform> possibleSpawnPoints; // Positions possibles du meneur
    public float maxDistanceToWin = 3f; // Distance maximale pour gagner
    public float gameDuration = 60f; // Durée totale du jeu en secondes
    public float minTimeBetweenStates = 1f; // Temps minimum entre changements
    public float maxTimeBetweenStates = 5f; // Temps maximum entre changements
    public float startDelay = 5f; // Délai avant le début du jeu
    public string loseSceneName = "Hub"; // Nom de la scène de défaite

    [Header("UI References")]
    public TMP_Text countdownText; // Texte pour le compte à rebours
    public TMP_Text gameStateText; // Texte pour l'état du jeu (Red/Green Light)
    public TMP_Text instructionsText; // Texte pour les instructions
    public Canvas Lights;
    public Image Red;
    public Image Green;

    [Header("Game State")]
    public bool isGreenLight = false;
    public bool gameIsActive = false;
    public bool gameStarting = false;
    private Transform currentGameMaster;

    private float gameTimer;
    private float startTimer;
    private List<Photon.Realtime.Player> playersInGame = new List<Photon.Realtime.Player>();

    private void Start()
    {
        if (gameStateText != null) gameStateText.text = "";
        if (Lights != null) Lights.enabled = false;
        // Start Game
        StartGameCountdown();
    }

    // Start initial the CountDown
    public void StartGameCountdown()
    {
        if (gameStarting || gameIsActive) return;

        gameStarting = true;
        startTimer = startDelay;

        // Show initials instructions
        if (instructionsText != null)
        {
            instructionsText.text = "Beware it will start soon ! Search the Doll when it's GREEN LIGHT, but FREEZE when it's RED LIGHT!";
        }
        StartCoroutine(StartCountdownCoroutine());
    }

    private IEnumerator StartCountdownCoroutine()
    {
        while (startTimer > 0)
        {
            startTimer -= Time.deltaTime;

            // Update the textCountDown
            if (countdownText != null)
            {
                countdownText.text = $"Game starts in: {Mathf.CeilToInt(startTimer)}";
            }

            yield return null;
        }

        // Update the game after the countDown
        StartGame();
    }

    private void StartGame()
    {
        
        gameStarting = false;
        gameIsActive = true;
        gameTimer = gameDuration;

        // Hide the countDown and the instructions
        if (countdownText != null) countdownText.text = "";
        if (instructionsText != null) instructionsText.text = "";

        // Spawn of the game master (Doll)
        SpawnGameMaster();
        if (Lights != null) Lights.enabled = true;

        // Start coroutines
        StartCoroutine(GameTimerCoroutine());
        StartCoroutine(LightSwitchCoroutine());
    }

    private void SpawnGameMaster()
    {
        try
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Not master client, won't spawn game master");
                return;
            }

            if (gameMasterPrefab == null)
            {
                Debug.LogError("Game Master Prefab is not assigned!");
                return;
            }

            if (possibleSpawnPoints == null || possibleSpawnPoints.Count == 0)
            {
                Debug.LogError("No spawn points available for Game Master");
                return;
            }

            Transform spawnPoint = possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Count)];
            GameObject master = PhotonNetwork.Instantiate(gameMasterPrefab.name, spawnPoint.position, spawnPoint.rotation);
            
            currentGameMaster = master.transform;
            photonView.RPC("SetGameMaster", RpcTarget.Others, master.GetComponent<PhotonView>().ViewID);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning Game Master: {e.Message}");
        }
    }

    [PunRPC]
    private void SetGameMaster(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null)
        {
            currentGameMaster = view.transform;
        }
    }

    private IEnumerator GameTimerCoroutine()
    {
        while (gameTimer > 0 && gameIsActive)
        {
            gameTimer -= Time.deltaTime;

            // Update Timer
            if (countdownText != null)
            {
                countdownText.text = $"Time remaining: {Mathf.FloorToInt(gameTimer)}";
            }

            // Check player's distance
            CheckPlayersDistance();

            yield return null;
        }

        // End the game when the timer reach 0
        EndGame();
    }

    private IEnumerator LightSwitchCoroutine()
    {
        // Wait before the first changement
        yield return new WaitForSeconds(1f);

        while (gameIsActive)
        {
            // Change the light
            isGreenLight = !isGreenLight;
            if (!isGreenLight) Green.enabled = false;
            else Green.enabled = true;

            // Synchronise clients state
            //photonView.RPC("SyncLightState", RpcTarget.All, isGreenLight);

            // Wait a random time for the light changement
            float waitTime = Random.Range(minTimeBetweenStates, maxTimeBetweenStates);
            yield return new WaitForSeconds(waitTime);
        }
    }

    [PunRPC]
    private void SyncLightState(bool state)
    {
        isGreenLight = state;
        
        // Update UI
        if (gameStateText != null)
        {
            gameStateText.text = isGreenLight ? "GREEN LIGHT - RUN!" : "RED LIGHT - FREEZE!";
            gameStateText.color = isGreenLight ? Color.green : Color.red;
        }

        Debug.Log(isGreenLight ? "GREEN LIGHT!" : "RED LIGHT!");
    }

    private void CheckPlayersDistance() // Corriger
    {
        if (!PhotonNetwork.IsMasterClient || currentGameMaster == null) return;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            GameObject playerObj = (GameObject)player.TagObject;
            if (playerObj == null) continue;

            float distance = Vector3.Distance(playerObj.transform.position, currentGameMaster.position);

            if (distance <= maxDistanceToWin)
            {
                // One player won
                photonView.RPC("PlayerWon", player);
            }
            else if (!isGreenLight && IsPlayerMoving(playerObj))
            {
                // One player move when the light is red
                gameStateText.text = "T'es guez";
                photonView.RPC("PlayerLost", player);
            }
        }
    }

    private bool IsPlayerMoving(GameObject player)  // Corriger
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            return true;
        }
        return false;
    }

    [PunRPC]
    private void PlayerWon()
    {
        Debug.Log("You won the game!");
        // Load victory scene
    }

    [PunRPC]
    private void PlayerLost()
    {
        Debug.Log("You lost! Returning to " + loseSceneName);
        // Just a debug message
        // Later : PhotonNetwork.LoadLevel(loseSceneName);
    }

    private void EndGame()
    {
        gameIsActive = false;
        StopAllCoroutines();
        Lights.enabled = false;

        Debug.Log("Game ended!");

        if (countdownText != null) countdownText.text = "";
        // Update UI
        if (gameStateText != null)
        {
            gameStateText.text = "GAME OVER";
        }

        // Destroy the game master
        if (currentGameMaster != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(currentGameMaster.gameObject);
        }
    }
}