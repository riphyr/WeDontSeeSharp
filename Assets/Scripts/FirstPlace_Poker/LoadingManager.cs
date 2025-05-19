using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject loadingUI;
    public Slider progressBar;
    public TMP_Text percentageText;
    public Image logo;
    public float pulseSpeed = 1.5f;
    public float pulseScale = 1.1f;

    private Vector3 originalScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Évite les doublons
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Garde le manager entre les scènes

        if (loadingUI != null)
            loadingUI.SetActive(false);
    }

    private void Start()
    {
        if (logo != null)
            originalScale = logo.transform.localScale;
    }

    private void Update()
    {
        if (loadingUI != null && loadingUI.activeSelf)
            AnimateLogo();
    }

    /// <summary>
    /// Lance le chargement d'une nouvelle scène avec l'écran de chargement.
    /// </summary>
    public void StartLoading(string sceneToLoad)
    {
        if (Instance != this)
        {
            Debug.LogWarning("Tentative d'appel à StartLoading sur une instance invalide.");
            return;
        }

        if (loadingUI != null)
            loadingUI.SetActive(true);
        ResetUI();

        StartCoroutine(WaitAndLoad(sceneToLoad));
    }
    
    private void ResetUI()
    {
        if (progressBar != null)
            progressBar.value = 0;

        if (percentageText != null)
            percentageText.text = "0%";
    }

    private IEnumerator WaitAndLoad(string sceneToLoad)
    {
        yield return null; // Laisse un frame au système pour activer l'UI

        yield return StartCoroutine(LoadAsyncOperation(sceneToLoad));

        if (loadingUI != null)
            loadingUI.SetActive(false);
    }

    private void AnimateLogo()
    {
        if (logo != null)
        {
            float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1);
            logo.transform.localScale = originalScale * scale;
        }
    }

    private IEnumerator LoadAsyncOperation(string sceneToLoad)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        float displayProgress = 0f;

        while (!asyncLoad.isDone)
        {
            float targetProgress = asyncLoad.progress < 0.9f
                ? Mathf.Clamp01(asyncLoad.progress / 0.9f)
                : 1f;

            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.deltaTime * 0.5f);

            if (progressBar != null)
                progressBar.value = displayProgress;

            if (percentageText != null)
                percentageText.text = Mathf.RoundToInt(displayProgress * 100f) + "%";

            if (displayProgress >= 1f && asyncLoad.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}