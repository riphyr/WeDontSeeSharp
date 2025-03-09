using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MiniGameManager : MonoBehaviour
{
    private GameObject[] _playerObjects;

    public GameObject targetRoom;

    public MiniGame CurrentMiniGame;
    private bool isMiniGameRunning = false;

    // Reference to the text UI
    public Text playerCountText;

    private bool hasPlayerEnteredRoom = false;
    

    public void Start()
    {
        _playerObjects = GameObject.FindGameObjectsWithTag("Player");
    }
    private bool AreAllPlayersInRoom()
    {
        foreach (var player in _playerObjects)
        {
            if (!IsPlayerInRoom(player))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsPlayerInRoom(GameObject player)
    {
        return targetRoom.GetComponent<Collider>().bounds.Contains(player.transform.position);
    }

    private int CountPlayersInRoom()
    {
        int count = 0;
        foreach (var player in _playerObjects)
        {
            if (IsPlayerInRoom(player))
            {
                count++;
            }
        }
        return count;
    }

    // Update text UI
    private void UpdatePlayerCountUI()
    {
        int playersInRoom = CountPlayersInRoom();
        int totalPlayers = _playerObjects.Length;

        playerCountText.text = $"Players : {playersInRoom} / {totalPlayers}";

        if (playersInRoom == totalPlayers)
        {
            playerCountText.color = Color.green;
        }
        else
        {
            playerCountText.color = Color.red;
        }
    }

    private void LaunchMiniGame()
    {
        if (AreAllPlayersInRoom())
        {
            CurrentMiniGame.isInitialized = true;
            isMiniGameRunning = true;

            // Deactivate player count when game launch
            playerCountText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!hasPlayerEnteredRoom && CountPlayersInRoom() > 0)
        {
            hasPlayerEnteredRoom = true;
            playerCountText.gameObject.SetActive(true); // Activate counter
        }

        if (hasPlayerEnteredRoom)
        {
            UpdatePlayerCountUI();
        }

        if (!isMiniGameRunning && AreAllPlayersInRoom())
        {
            LaunchMiniGame();
        }
    }
}