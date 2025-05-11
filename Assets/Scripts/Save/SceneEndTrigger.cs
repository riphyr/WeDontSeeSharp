using UnityEngine;
using Photon.Pun;

public class SceneEndTrigger : MonoBehaviourPunCallbacks
{
    [Header("Nom de la scène suivante (Unity Build Settings)")]
    [SerializeField] private string nextSceneName;

    private bool hasTriggered = false;

    public void Trigger()
    {
        if (hasTriggered) return;

        hasTriggered = true;
        photonView.RPC(nameof(RPC_TriggerSceneEnd), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_TriggerSceneEnd()
    {
        GameSaveManager.Save(GameManager.Instance.CurrentGameData);
        PhotonNetwork.LoadLevel(nextSceneName);
    }
}

