using UnityEngine;
using Photon.Pun;
using System.Collections;

public class SafeDoor : MonoBehaviourPun
{
    public Transform openPosition;
    public float openSpeed = 1f;
    private bool isOpen = false;
    
    public void OpenDoor()
    {
        if (!isOpen)
        {
            StartCoroutine(OpenSequence());
        }
    }
    
    private IEnumerator OpenSequence()
    {
        isOpen = true;
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, openPosition.position, elapsedTime);
            elapsedTime += Time.deltaTime * openSpeed;
            yield return null;
        }
        transform.position = openPosition.position;
    }
}