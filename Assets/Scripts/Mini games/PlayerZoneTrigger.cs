using UnityEngine;
using Photon.Pun;

public class PlayerZoneTrigger : MonoBehaviour
{
    [Tooltip("Reference to the Collider")]
    public Collider triggerZone;

    [Tooltip("Script to start")]
    public MonoBehaviour scriptToExecute;

    [Tooltip("Function to call")]
    public string functionName = "ExecuteAction";

    private void Update()
    {
        if (PhotonNetwork.InRoom && AllPlayersInZone())
        {
            ExecuteTargetScript();
        }
    }

    private bool AllPlayersInZone()
    {
        if (triggerZone == null)
        {
            Debug.LogError("No zone definied!");
            return false;
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int playersInZone = 0;

        foreach (GameObject player in players)
        {
            if (triggerZone.bounds.Contains(player.transform.position))
            {
                playersInZone++;
            }
        }

        Debug.Log($"Players : {playersInZone}/{PhotonNetwork.CurrentRoom.PlayerCount}");

        return playersInZone >= PhotonNetwork.CurrentRoom.PlayerCount;
    }

    private void ExecuteTargetScript()
    {
        if (scriptToExecute == null)
        {
            Debug.LogError("No script definied!");
            return;
        }

        if (string.IsNullOrEmpty(functionName))
        {
            Debug.LogError("No function specified!");
            return;
        }

        scriptToExecute.Invoke(functionName, 0f);
        this.enabled = false;  
    }

    private void OnDrawGizmos()
    {
        if (triggerZone != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(triggerZone.bounds.center, triggerZone.bounds.size);
        }
    }  
}