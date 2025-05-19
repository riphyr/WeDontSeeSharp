using UnityEngine;
using Photon.Pun;

public class SceneEndTrigger : MonoBehaviourPunCallbacks
{
    public ReturnToHubZone returnToHubZone;

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
        returnToHubZone.TriggerReturnForEveryone();
    }
}

