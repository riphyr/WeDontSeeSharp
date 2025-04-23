using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalZone : MonoBehaviour
{
    public string sceneToLoad = "NomDeLaScene"; // Remplace par le nom réel de ta scène

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Stopper la musique
            GameObject music = GameObject.Find("MusicManager");
            if (music != null)
            {
                Destroy(music);
            }

            // Charger la nouvelle scène
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

   
