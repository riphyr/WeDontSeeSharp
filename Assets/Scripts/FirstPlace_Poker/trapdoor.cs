using System.Collections;
using UnityEngine;

// Ce script n'est pas adapter pour le multijoueur, en revanche il n'est pas compliqué d'integerer phton a ce script.

public class Trapdoor : MonoBehaviour
{
    [Header("UI Elements")] 
    public GameObject Text;

    private bool interactable = false;
    public float speed = 2f;
    public Transform solGauche, posGauche;
    private bool isMoving = false;

    void OnTriggerEnter(Collider other)
    {
        //if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;
        Text.SetActive(true);
        interactable = true;
    }

    void OnTriggerExit(Collider other)
    {
        //if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;
        Text.SetActive(false);
        interactable = false;
    }

    void Update()
    {
        if (interactable && Input.GetKeyDown(KeyCode.E) && !isMoving) // faut recuperer le setting du joueur ici !!!
        {
            Text.SetActive(false); 
            StartCoroutine(TrapCoroutine()); 
        }
    }

    IEnumerator TrapCoroutine()
    {
        isMoving = true;
        float duration = 5f;
        float disparition = 0f;
        Vector3 startPos = solGauche.position;
        Vector3 endPos = posGauche.position;
        while (disparition < duration)
        {
            solGauche.position = Vector3.Lerp(startPos, endPos, disparition / duration);
            disparition += Time.deltaTime;
             yield return null;
        }

        solGauche.position = endPos; 
        isMoving = false; 
    }
}

