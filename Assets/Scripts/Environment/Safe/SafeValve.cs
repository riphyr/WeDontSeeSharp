using UnityEngine;
using Photon.Pun;
using System.Collections;

public class SafeValve : MonoBehaviourPun
{
    public AudioClip unlockSound;
    public AudioClip failSound;
    private bool isUnlocked = false;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    public void SetUnlocked(bool state)
    {
        isUnlocked = state;
    }
    
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryUnlock();
        }
    }
    
    void TryUnlock()
    {
        if (isUnlocked)
        {
            photonView.RPC("Unlock", RpcTarget.All);
        }
        else
        {
            photonView.RPC("FailUnlock", RpcTarget.All);
        }
    }
    
    [PunRPC]
    void Unlock()
    {
        if (unlockSound != null)
            audioSource.PlayOneShot(unlockSound);
        FindObjectOfType<SafeDoor>().OpenDoor();
    }
    
    [PunRPC]
    void FailUnlock()
    {
        if (failSound != null)
            audioSource.PlayOneShot(failSound);
    }
}