using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManagerLose : MonoBehaviour
{
    public AudioClip musicClip;
    private AudioSource audioSource;

    // Nom des scènes où la musique doit être active
    public string[] scenesToPlayMusic;

    void Awake()
    {
        // Empêche les doublons
        if (FindObjectsOfType<MusicManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        //audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;

        // Écoute les changements de scène
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Joue ou pas selon la scène de départ
        CheckSceneAndPlay(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneAndPlay(scene);
    }

    private void CheckSceneAndPlay(Scene scene)
    {
        bool shouldPlay = false;

        foreach (string sceneName in scenesToPlayMusic)
        {
            if (scene.name == sceneName)
            {
                shouldPlay = true;
                break;
            }
        }

        if (shouldPlay)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    public void StopMusic(float fadeDuration = 0f)
    {
        if (fadeDuration > 0f)
        {
            StartCoroutine(FadeOutAndStop(fadeDuration));
        }
        else
        {
            audioSource.Stop();
        }
    }

    private System.Collections.IEnumerator FadeOutAndStop(float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}

