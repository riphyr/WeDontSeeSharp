using UnityEngine;
using Photon.Pun;

public class SceneEndTrigger : MonoBehaviourPunCallbacks
{
    [Header("Nom de la scène suivante (Unity Build Settings)")]
    [SerializeField] private string nextSceneName;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasTriggered)
            return;

        hasTriggered = true;
        photonView.RPC(nameof(RPC_TriggerSceneEnd), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_TriggerSceneEnd()
    {
        Debug.Log("[SceneEndTrigger] Fin de scène — changement pour tous les joueurs.");
        GameSaveManager.Save(GameManager.Instance.CurrentGameData);

        PhotonNetwork.LoadLevel(nextSceneName);
    }

}