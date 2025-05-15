using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;
using Photon.Pun;

public class ReturnToHubZone : MonoBehaviourPun
{
    public string hubSceneName = "FirstPlace_Poker2";
    //public LoadingManager loadingManager;

    [PunRPC]
    public void ReturnToHub()
    {
        if (!photonView.IsMine) return;

        PlayerPrefs.SetString("SpawnPointID", "hub");
        PlayerPrefs.Save();

        FindObjectOfType<HubManager>()?.PlayerReturned(PhotonNetwork.LocalPlayer.UserId);
        LoadingManager.Instance.StartLoading(hubSceneName);
    }

    // Méthode appelée via un script central pour démarrer la téléportation pour tous
    public void TriggerReturnForEveryone()
    {
        photonView.RPC("ReturnToHub", RpcTarget.All);
    }
}



