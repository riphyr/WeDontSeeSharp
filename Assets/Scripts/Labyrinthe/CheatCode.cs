using UnityEngine;
using System.Collections;


public class CheatCode : MonoBehaviour
{
    public Vector3 teleportDestination = new Vector3(0f, 5f, 0f);
    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Touche O' pressée. Téléportation lancée.");
            StartCoroutine(TeleportWithDelay());
        }
    }

    IEnumerator TeleportWithDelay()
    {
        Debug.Log("Désactivation du CharacterController...");
       // cc.enabled = false;
        yield return new WaitForSeconds(0.05f); // petit délai pour que Unity respire

        Debug.Log("Changement de position...");
        transform.position = teleportDestination;
       
        transform.rotation = Quaternion.Euler(0f, 360f, 0f); //rotation de 90 degres
        
        yield return null; // attendre une frame de plus

        Debug.Log("Réactivation du CharacterController...");
        //cc.enabled = true;

        Debug.Log("Position actuelle : " + transform.position);
    }
}