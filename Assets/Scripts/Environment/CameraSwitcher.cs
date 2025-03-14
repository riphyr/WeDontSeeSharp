using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class CameraSwitcher : MonoBehaviourPun
{
    public Camera[] securityCameras;   // 📹 Liste des caméras de surveillance
    public RenderTexture[] renderTextures; // 🎥 Render Textures associées
    public Material screenMaterial;    // 🖥️ Matériau de l’écran de surveillance

    private int currentCameraIndex = 0;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient) // 🔰 Initialisation par le MasterClient
        {
            photonView.RPC("RPC_SwitchCamera", RpcTarget.AllBuffered, 0);
        }
    }

    public void NextCamera()
    {
        if (!photonView.IsMine)
        {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer); // 📡 Prendre temporairement le contrôle
        }

        Debug.Log("📸 Changement de caméra demandé !");
        int newIndex = (currentCameraIndex + 1) % securityCameras.Length;
        photonView.RPC("RPC_SwitchCamera", RpcTarget.AllBuffered, newIndex);
    }

    [PunRPC]
    public void RPC_SwitchCamera(int index)
    {
        if (index < 0 || index >= securityCameras.Length) return;

        // 🔄 Désactiver uniquement le rendu des autres caméras, mais pas le GameObject
        foreach (Camera cam in securityCameras)
        {
            cam.enabled = false; // Désactive uniquement le rendu de la caméra
        }

        // ✅ Active la nouvelle caméra
        securityCameras[index].enabled = true;

        // 🎥 Met à jour l’image sur l’écran de surveillance
        screenMaterial.mainTexture = renderTextures[index];

        currentCameraIndex = index;

        Debug.Log($"📷 Caméra activée : {securityCameras[index].name}");
    }
}