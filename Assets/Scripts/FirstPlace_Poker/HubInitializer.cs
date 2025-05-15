using UnityEngine;
using Photon.Pun;

public class HubInitializer : MonoBehaviourPunCallbacks
{
    void Start()
    {
        string userId = PhotonNetwork.LocalPlayer.UserId;
        string lastLevelID = LevelTransitionData.LastLevelID;

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(lastLevelID))
        {
            GameProgressManager.Instance.RegisterReturnToHub(userId, lastLevelID);
        }
    }
}

