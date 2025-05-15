using System.Collections;
using UnityEngine;
using TMPro;

public class DoorTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI messageUI; 
    public string fullMessage = "Take a seat on the chairs in front of you"; 
    private bool messageDisplayed = false; 
    private bool isCoroutineRunning = false; 

    private void Start()
    {
        messageUI.text = "";
        messageUI.gameObject.SetActive(false); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!messageDisplayed && !isCoroutineRunning) 
        {
            StartCoroutine(DisplayMessage()); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
    }

    private IEnumerator DisplayMessage()
    {
        isCoroutineRunning = true;  
        messageUI.gameObject.SetActive(true);  
        messageUI.text = "";  

        for (int i = 0; i < fullMessage.Length; i++)
        {
            messageUI.text += fullMessage[i];  
            yield return new WaitForSeconds(0.05f);  
        }

        yield return new WaitForSeconds(5f); 
        messageUI.gameObject.SetActive(false);  
        messageDisplayed = true;  
        isCoroutineRunning = false;  
    }
}


