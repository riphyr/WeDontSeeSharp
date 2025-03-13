using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class UVObject : MonoBehaviour
{
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material); // Duplique le matériau
            renderer.material.SetFloat("_UVIntensity", 0f);
        }
    }
}