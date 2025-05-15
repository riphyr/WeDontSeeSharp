using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
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
        PhotonNetwork.LoadLevel("labyrinthe");
        //PhotonNetwork.LoadLevel("Test");
        //PhotonNetwork.LoadLevel("FirstPlace_Poker2");
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