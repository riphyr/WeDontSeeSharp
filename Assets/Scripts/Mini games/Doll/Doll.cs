using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MiniGame
{
    public static List<GameObject> Players; // Listof reference to players
    public GameObject doll; // Reference to the doll
    public float minCheckInterval = 2f; // Minimum interval between checks
    public float maxCheckInterval = 5f; // Maximum interval between checks
    public float dollRotationSpeed = 100f; // Speed of Doll's rotation
    public float lookDuration = 2f;
    
    public float timeLimit = 30f; // Limit time to win

    public Text countdownText;
    public Text timerText; // Reference to the UI Text for display

    private bool _isCounting = false; // Is the Doll Checking
    private bool _isGameOver = false; // Is the game finished
    private float _nextCheckTime; // Time until the next check
    private Dictionary<GameObject, Vector3> _playerPreviousPositions; // The previous players position for the detection

    private Vector3 startPos = Players[0].transform.position; // Need changements depends on the game

    private float currentTime; // Time left

    public override void Start()
    {
        // Initialize the previous player position
        _playerPreviousPositions = new Dictionary<GameObject, Vector3>();

        foreach (var player in Players)
        {
            _playerPreviousPositions[player] = player.transform.position;
        }


        // Set the time of the first check
        SetNextCheckTime();

        // Initialize the time left
        currentTime = timeLimit;

        // Update the time display
        UpdateTimerUI();
        
        countdownText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isInitialized) return;
        
        if (_isGameOver) return;
        
        // Make the doll act
        if (Time.time >= _nextCheckTime)
        {
            DollCheck();
            SetNextCheckTime();
        }
        foreach (var player in Players)
        {
            // Check if the player reach the doll
            if (Vector3.Distance(player.transform.position, doll.transform.position) < 2f)
            {
                EndGame(true); // Players won
            }
        }

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime; // Decrease time left
            UpdateTimerUI(); // Update time's display
        }
        else
        {
            EndGame(false);
        }
    }

    void DollCheck()
    {
        StartCoroutine(RotateAndCheck());
    }
    
    //Close the room
    void CloseDoor()
    {
        //Close the door
    }

    System.Collections.IEnumerator RotateAndCheck()
    {
        _isCounting = true;
        
        float targetAngle = doll.transform.eulerAngles.y - 90f;
        while (doll.transform.eulerAngles.y > targetAngle + 1f)
        {
            doll.transform.Rotate(Vector3.up, - dollRotationSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(lookDuration);

        CheckPlayerMovement();

        targetAngle = doll.transform.eulerAngles.y + 90f;
        while (doll.transform.eulerAngles.y < targetAngle - 1f)
        {
            doll.transform.Rotate(Vector3.up, dollRotationSpeed * Time.deltaTime);
            yield return null;
        }

        _isCounting = false;
    }

    System.Collections.IEnumerator ShowCountdown()
    {
        // Activer le texte de décompte
        countdownText.gameObject.SetActive(true);

        // Afficher "1"
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        // Afficher "2"
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        // Afficher "3"
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        // Afficher "Soleil !"
        countdownText.text = "Soleil !";
        yield return new WaitForSeconds(1f);

        // Désactiver le texte de décompte
        countdownText.gameObject.SetActive(false);
    }
    void CheckPlayerMovement()
    {
        // Vérifier si un joueur a bougé depuis la dernière vérification
        foreach (var player in Players)
        {
            if (player.transform.position != _playerPreviousPositions[player])
            {
                EndGame(false); // Un joueur a bougé, tout le monde perd
                return;
            }
        }

        // Mettre à jour les positions précédentes des joueurs
        foreach (var player in Players)
        {
            _playerPreviousPositions[player] = player.transform.position;
        }
    }
    void SetNextCheckTime()
    {
        // Interval for the next verification
        _nextCheckTime = Time.time + Random.Range(minCheckInterval, maxCheckInterval);
    }

    void UpdateTimerUI()
    {
        // Update the display's time in the UI
        if (timerText != null)
        {
            timerText.text = "Time left : " + Mathf.CeilToInt(currentTime).ToString();
        }
    }
    
    void EndGame(bool playerWon)
    {
        enabled = false;
        _isGameOver = true;
        if (playerWon)
        {
            Debug.Log("Player win");
        }
        else
        {
            Debug.Log("Players lose");
        }
    }
}