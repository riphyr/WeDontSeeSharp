using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [Header("Fields create")]
    public TMP_InputField createInput;
    public GameObject createPanel;
    
    [Header("Fields join")]
    public TMP_InputField joinInput;
    public GameObject joinPanel;
    public GameObject errorMessage;
    
    [Header("Fields load")]
    public TMP_InputField loadInput;
    public GameObject loadPanel;
    public GameObject errorMessageLoad;
    public bool isResuming = false;
    
    [Header("First scene name")]
    [SerializeField] private string firstSceneName;

    public void CreateRoom(bool isMultiplayer)
    {
        PhotonNetwork.CreateRoom(createInput.text);

        var save = new GameSaveData
        {
            isMultiplayer = isMultiplayer,
            roomName = isMultiplayer ? createInput.text : "",
            currentScene = "",
            playTime = 0f
        };
    
        GameSaveManager.Save(save);
    }


    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public void ResumeRoom(bool isMultiplayer)
    {
        isResuming = true;
        if (GameSaveManager.isSaveAvailable())
        {
            PhotonNetwork.CreateRoom(loadInput.text);
            
        }
        else
            OnJoinRoomFailed(0, "");
    }

    public override void OnJoinedRoom()
    {
        if (!isResuming)
            PhotonNetwork.LoadLevel(firstSceneName);
        else
        {
            isResuming = false;
            GameSaveData save = GameSaveManager.Load();
            string scene = save.currentScene;
            
            if (scene == "")
                PhotonNetwork.LoadLevel(firstSceneName);
            else
                PhotonNetwork.LoadLevel(scene);
        }
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (!isResuming)
            errorMessage.SetActive(true);
        else
            errorMessageLoad.SetActive(true);
    }

    public void OnInputFieldChanged()
    {
        if (errorMessage.activeSelf)
            errorMessage.SetActive(false);
        if (errorMessageLoad.activeSelf)
            errorMessageLoad.SetActive(false);
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