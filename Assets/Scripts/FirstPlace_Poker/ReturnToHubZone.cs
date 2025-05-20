using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;
using Photon.Pun;

public class ReturnToHubZone : MonoBehaviourPun
{
    public string hubSceneName = "FirstPlace_Poker2";

    [PunRPC]
    public void ReturnToHub()
    {
        if (!photonView.IsMine) return;

        PlayerPrefs.SetString("SpawnPointID", "hub");
        PlayerPrefs.Save();

        LoadingManager.Instance.StartLoading(hubSceneName);
    }

    public void TriggerReturnForEveryone()
    {
        photonView.RPC("ReturnToHub", RpcTarget.All);
    }
}



