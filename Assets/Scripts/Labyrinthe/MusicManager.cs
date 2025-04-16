using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip musicClip; // Mets ici ta musique dans l’inspector
    private AudioSource audioSource;

    void Awake()
    {
        // Empêche les doublons quand on change de scène
        if (FindObjectsOfType<MusicManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Persiste entre les scènes (sauf si on le détruit nous-même)
        DontDestroyOnLoad(gameObject);

        // Ajoute AudioSource si nécessaire
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // Paramètre et joue la musique
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = true; // ← Joue dès que le GameObject est activé
        audioSource.volume = 0.5f;

        // Pas besoin d’appeler Play() si playOnAwake est activé, mais on peut le faire pour être sûr
        audioSource.Play();
    }
}