using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // Importer TextMeshPro
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput; 
    public GameObject errorMessage;

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameTest");
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        errorMessage.SetActive(true);
    }

    public void OnInputFieldChanged()
    {
        if (errorMessage.activeSelf)
            errorMessage.SetActive(false);
    }
}