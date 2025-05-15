using UnityEngine;
using Photon.Pun;

public class SceneExitUnlock : MonoBehaviourPun
{
    void Start()
    {
        DoorController.SetDoorLock(false); // déverrouille la porte
        Debug.Log("🔓 Porte déverrouillée au retour !");
    }
}