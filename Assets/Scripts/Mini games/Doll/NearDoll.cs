using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class NearDoll : MonoBehaviourPun
{
    [Header("Game Reference")]
    [SerializeField] private RedLightGreenLightGame gameManager;

    private void Start()
    {
        // Trouve automatiquement le game manager si non assigné
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<RedLightGreenLightGame>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView != null && playerView.IsMine)
        {
            // Déclenche la victoire uniquement pour le joueur concerné
            gameManager?.photonView.RPC("PlayerWon", playerView.Owner);
        }
    }
}