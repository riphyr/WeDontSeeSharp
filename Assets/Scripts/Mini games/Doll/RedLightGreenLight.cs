using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;

public class RedLightGreenLight : MonoBehaviourPunCallbacks
{
    [Header("Game Parameters")]
    public float minCountdownTime = 3f;
    public float maxCountdownTime = 10f;
    public float rotationSpeed = 90f;
    public float winDistance = 1f;
    public float startPositionThreshold = 0.2f;

    [Header("References")]
    public Transform gameMaster;
    public TextMeshProUGUI countdownText;
    public Transform[] startPositions;

    private bool isCounting = false;
    private bool playersCanMove = false;
    private float currentCountdown;
    private Coroutine countdownCoroutine;
    private Vector3[] playerPositions;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            playerPositions = new Vector3[PhotonNetwork.CurrentRoom.PlayerCount];
            StartNewCountdown();
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (!playersCanMove)
        {
            gameMaster.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        if (!playersCanMove && isCounting)
        {
            CheckMovingPlayers();
        }

        CheckWinningPlayers();
    }

    [PunRPC]
    public void StartNewCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        currentCountdown = Random.Range(minCountdownTime, maxCountdownTime);
        countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        isCounting = true;
        playersCanMove = true;
        photonView.RPC("UpdateCountdownText", RpcTarget.All, "1, 2, 3... MOVE!");

        yield return new WaitForSeconds(currentCountdown);

        isCounting = false;
        playersCanMove = false;
        photonView.RPC("UpdateCountdownText", RpcTarget.All, "SOLEIL!");
        photonView.RPC("StorePlayerPositions", RpcTarget.All);

        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.IsMasterClient)
        {
            StartNewCountdown();
        }
    }

    [PunRPC]
    private void UpdateCountdownText(string text)
    {
        countdownText.text = text;
    }

    [PunRPC]
    private void StorePlayerPositions()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetPhotonView().IsMine)
            {
                playerPositions[i] = players[i].transform.position;
            }
        }
    }

    private void CheckMovingPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (Vector3.Distance(players[i].transform.position, playerPositions[i]) > startPositionThreshold)
            {
                photonView.RPC("SendPlayerToStart", players[i].GetPhotonView().Owner, i % startPositions.Length);
                break;
            }
        }
    }

    [PunRPC]
    private void SendPlayerToStart(int startIndex)
    {
        transform.position = startPositions[startIndex].position;
    }

    private void CheckWinningPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (Vector3.Distance(player.transform.position, gameMaster.position) < winDistance)
            {
                photonView.RPC("EndGame", RpcTarget.All, player.GetPhotonView().Owner.NickName + " a gagné!");
                break;
            }
        }
    }

    [PunRPC]
    private void EndGame(string message)
    {
        countdownText.text = message;
        enabled = false;
    }

    public bool CanPlayerMove()
    {
        return playersCanMove;
    }
}