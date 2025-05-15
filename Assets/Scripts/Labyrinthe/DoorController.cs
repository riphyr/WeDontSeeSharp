using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class DoorController : MonoBehaviourPunCallbacks
{
    public Animator doorAnimator;
    public Collider teleportTriggerZone;
    public string doorLockKey = "DoorLocked";

    void Start()
    {
        // On commence avec la porte verrouillée
        teleportTriggerZone.enabled = false;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return; // seul le Master contrôle l'état global de la porte

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!IsDoorLocked())
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("🚫 Porte verrouillée !");
            }
        }
    }

    void OpenDoor()
    {
        Debug.Log("🚪 Ouverture de la porte");
        doorAnimator.SetTrigger("Open");
        teleportTriggerZone.enabled = true;
    }

    public static bool IsDoorLocked()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("DoorLocked", out object locked))
        {
            return (bool)locked;
        }
        return false;
    }

    public static void SetDoorLock(bool locked)
    {
        Hashtable props = new Hashtable();
        props["DoorLocked"] = locked;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}