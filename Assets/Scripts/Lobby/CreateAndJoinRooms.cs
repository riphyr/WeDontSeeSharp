using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public GameObject createPanel;
    public GameObject joinPanel;
    public GameObject errorMessage;

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text);

        var save = new GameSaveData
        {
            isMultiplayer = true,
            roomName = createInput.text,
            currentScene = "HouseLvl",
            playTime = 0f
        };
    
        GameSaveManager.Save(save);
    }


    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("HouseLvl");
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (createPanel.activeSelf && !string.IsNullOrWhiteSpace(createInput.text))
                PhotonNetwork.CreateRoom(createInput.text);
            else if (joinPanel.activeSelf && !string.IsNullOrWhiteSpace(joinInput.text))
                PhotonNetwork.JoinRoom(joinInput.text);
        }
    }
}