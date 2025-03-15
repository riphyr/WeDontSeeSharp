using UnityEngine;
using Photon.Pun;
using System.Collections;

public class SafeDial : MonoBehaviourPun
{
    public AudioClip clickSound;
    public int[] correctCombination;
    private int[] enteredCombination;
    private int currentStep = 0;
    private int currentNumber = 0;
    private bool isTurningRight = true;
    private AudioSource audioSource;
    
    void Start()
    {
        enteredCombination = new int[correctCombination.Length];
        audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        if (!photonView.IsMine) return;
        
        int direction = 0;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = -1;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = 1;
        
        if (direction != 0)
        {
            RotateDial(direction);
        }
    }
    
    void RotateDial(int direction)
    {
        if (isTurningRight != (direction > 0))
        {
            if (currentStep < correctCombination.Length)
            {
                enteredCombination[currentStep] = currentNumber;
                currentStep++;
            }
            isTurningRight = direction > 0;
        }
        
        currentNumber = (currentNumber + direction + 100) % 100; 
        transform.Rotate(0, 0, -direction * 9); 
        photonView.RPC("SyncRotation", RpcTarget.Others, transform.rotation);
        
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
        
        if (currentStep == correctCombination.Length - 1 && enteredCombination[currentStep] == correctCombination[currentStep])
        {
            photonView.RPC("UnlockSafe", RpcTarget.All);
        }
    }
    
    [PunRPC]
    void SyncRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
    
    [PunRPC]
    void UnlockSafe()
    {
        FindObjectOfType<SafeValve>().SetUnlocked(true);
    }
}