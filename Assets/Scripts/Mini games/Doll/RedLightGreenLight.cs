using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class RedLightGreenLightGame : MonoBehaviourPunCallbacks
{
    [Header("Game Settings")]
    //public GameObject gameMasterPrefab; // Préfab du meneur de jeu
    public GameObject GameMaster;
    public List<Transform> possibleSpawnPoints; // Positions possibles du meneur
    public float gameDuration = 60f; // Durée totale du jeu en secondes
    public float minTimeBetweenStates = 1f; // Temps minimum entre changements
    public float maxTimeBetweenStates = 5f; // Temps maximum entre changements
    public float startDelay = 5f; // Délai avant le début du jeu
    public string loseSceneName = "Hub"; // Nom de la scène de défaite
    public GameObject Cards;


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
    public NearDoll Doll;
    private bool isWon = false;

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
        isGreenLight = true;
        gameTimer = gameDuration;

        // Hide the countDown and the instructions
        if (countdownText != null) countdownText.text = "";
        if (instructionsText != null) instructionsText.text = "";

        // Spawn of the game master (Doll)
        TeleportGameMaster();
        if (Lights != null) Lights.enabled = true;

        // Start coroutines
        StartCoroutine(GameTimerCoroutine());
        StartCoroutine(LightSwitchCoroutine());
    }

    private void TeleportGameMaster()
    {
        Transform spawnPoint = possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Count)];
        GameMaster.transform.position = spawnPoint.position;
        GameMaster.transform.rotation = spawnPoint.rotation;
        currentGameMaster = GameMaster.transform;
        photonView.RPC("SetGameMaster", RpcTarget.Others, GameMaster.GetComponent<PhotonView>().ViewID);
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
            isGreenLight = !isGreenLight;
            if (!isGreenLight) Green.enabled = false;
            else Green.enabled = true;

            // Réactiver cette ligne pour synchroniser l'état
            photonView.RPC("SyncLightState", RpcTarget.All, isGreenLight);

            float waitTime = Random.Range(minTimeBetweenStates, maxTimeBetweenStates);
            yield return new WaitForSeconds(waitTime);
        }
    }

    [PunRPC]
    private void SyncLightState(bool state)
    {
        isGreenLight = state;
        Green.enabled = isGreenLight;
        Red.enabled = !isGreenLight;
    }

   private void CheckPlayersDistance()
{
    if (!PhotonNetwork.IsMasterClient || !gameIsActive) return;

    foreach (var player in PhotonNetwork.PlayerList)
    {
        GameObject playerObj = GetPlayerGameObject(player);
        if (playerObj == null) continue;

        UpdateMovementTracker(playerObj);

        // Vérification défaite
        if (!isGreenLight && ShouldPlayerLose(playerObj))
        {
            photonView.RPC("PlayerLost", player);
        }
    }
}

    private Dictionary<int, Vector3> lastPositions = new Dictionary<int, Vector3>();
    private Dictionary<int, float> lastCheckTimes = new Dictionary<int, float>();
    private const float MOVEMENT_THRESHOLD = 0.1f; // Minimum movement distance to consider
    private const float MIN_TIME_BETWEEN_CHECKS = 0.1f; // Prevent too frequent checks

private Dictionary<int, MovementTracker> movementTrackers = new Dictionary<int, MovementTracker>();

private class MovementTracker {
    public Vector3 greenLightStartPosition;
    public bool movedDuringRed;
    public float lastRedLightCheckTime;
}

    private void UpdateMovementTracker(GameObject player)
    {
        PhotonView view = player.GetComponent<PhotonView>();
        if (view == null) return;

        int playerId = view.ViewID;

        if (!movementTrackers.ContainsKey(playerId))
        {
            movementTrackers[playerId] = new MovementTracker();
        }

        var tracker = movementTrackers[playerId];

        if (isGreenLight)
        {
            // Réinitialiser le tracker en Green Light
            tracker.greenLightStartPosition = player.transform.position;
            tracker.movedDuringRed = false;
        }
        else
        {
            // Vérifier le mouvement SEULEMENT pendant Red Light
            float distance = Vector3.Distance(tracker.greenLightStartPosition, player.transform.position);
            tracker.movedDuringRed = tracker.movedDuringRed || (distance > 0.05f); // Seuil de 5cm
        }
    }
private bool ShouldPlayerLose(GameObject player) {
    PhotonView view = player.GetComponent<PhotonView>();
    if (view == null || isGreenLight) return false;

    int playerId = view.ViewID;
    if (!movementTrackers.ContainsKey(playerId)) return false;

    // Le joueur perd seulement s'il a bougé DURANT la phase Red Light
    return movementTrackers[playerId].movedDuringRed;
}

    private GameObject GetPlayerGameObject(Photon.Realtime.Player player)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in views)
        {
            if (view.Owner == player && view.IsMine)
            {
                return view.gameObject;
            }
        }
        return null;
    }

    [PunRPC]
    private void PlayerWon()
    {
        Debug.Log("You won the game!");
        isWon = true;
        EndGame();
    }

    [PunRPC]
    private void PlayerLost()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Only this player lost: " + PhotonNetwork.LocalPlayer.NickName);
            PhotonNetwork.LoadLevel(loseSceneName);
        }
    }

    private void EndGame()
    {
        gameIsActive = false;
        StopAllCoroutines();
        Lights.enabled = false;

        Debug.Log("Game ended!");

        if (countdownText != null) countdownText.text = "";

        if (!isWon)
        {
            if (gameStateText != null)
            {
                gameStateText.text = "GAME OVER";
            }

            if (currentGameMaster != null && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(currentGameMaster.gameObject);
            }
            photonView.RPC(nameof(StartCountdownRPC), RpcTarget.All);
        }
        else
        {
            if (currentGameMaster != null && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(currentGameMaster.gameObject);
            }

            photonView.RPC(nameof(RPC_ActivateCards), RpcTarget.All, true);

            instructionsText.text = "Go to the entrance to get the cards, you have 1 minute";
            StartCountdownRPCV();

        }

    }

    [PunRPC]
    private void RPC_ActivateCards(bool isActive)
    {
        Cards.SetActive(isActive);
    }

    [PunRPC]
    private void StartCountdownRPC()
    {
        StartCoroutine(CountdownAndReload());
    }

    private IEnumerator CountdownAndReload()
    {
        float countdown = 5f; // 5 secondes de décompte

        while (countdown > 0)
        {
            if (countdownText != null)
                countdownText.text = $"Returning in: {countdown:F1}s";

            yield return new WaitForSeconds(0.1f);
            countdown -= 0.1f;
        }
        PhotonNetwork.LoadLevel(loseSceneName);
    }
    
    [PunRPC]
    private void StartCountdownRPCV()
    {
        StartCoroutine(CountdownAndReloadV());
    }

    private IEnumerator CountdownAndReloadV()
    {
        float countdown = 60f;

        while (countdown > 0)
        {
            if (countdownText != null)
                countdownText.text = $"Returning in: {countdown:F1}s";

            yield return new WaitForSeconds(0.1f);
            countdown -= 0.1f;
        }
        PhotonNetwork.LoadLevel(loseSceneName);
    }
}