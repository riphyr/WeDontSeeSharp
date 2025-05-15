using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AIStoryManager : MonoBehaviourPun
{
    [Header("Core References")]
    [SerializeField] private LoreProgressManager loreManager;
    [SerializeField] private GhostInteractionController ghostController;
    [SerializeField] private GhostAI ghostAI;
    private InteractionScripts.Magnetophone currentMagnetophone = null;
    private float magnetoCheckInterval = 0.5f;
    private float magnetoCheckTimer = 0f;
    [SerializeField] private AudioSource ghostVoice;
    [SerializeField] private InteractionScripts.RemovablePlank plank;

    [Header("Timings")]
    [SerializeField] private float delayBeforeFirstAction = 5f;
    [SerializeField] private float timeBeforeFirstQuestion = 10f;
    [SerializeField] private float defaultAnswerTimeout = 15f;

	[Header("Magnetophone queue")]
	private Action<string> pendingQuestionHandler = null;
	private bool isWaitingForAnswer = false;

    [Header("Dialogue Clips")]
    public AudioClip firstQuestion; // truth ?
    public AudioClip responseYes_1; // drawers' desk
    public AudioClip responseNo_1; // drawers' desk

    public AudioClip afterPhotoTaken;

    public AudioClip secondQuestion; // regret ?
    public AudioClip thirdQuestion; // what happened ?
    public AudioClip responseYes_3; // diary in bedroom
    public AudioClip responseNo_3; // diary in bedroom
    public AudioClip responseNo_2; // diary in bedroom

    public AudioClip afterDiaryTaken;

    public AudioClip finalQuestion; // understand ?
    public AudioClip finalResponseYes; // listen tape
    public AudioClip finalResponseNo; // listen tape

    public AudioClip afterRadioPlayed;

    private bool awaitingResponse = false;
    private bool isScriptActive = false;

    void Start()
    {
        StartCoroutine(WaitForGhostActivation());
    }

    void Update()
    {
        if (!isScriptActive) 
		{
			return;
		}

        magnetoCheckTimer += Time.deltaTime;
        if (magnetoCheckTimer >= magnetoCheckInterval)
        {
            magnetoCheckTimer = 0f;
            FindActiveMagnetophone();
        }
    }

    void FindActiveMagnetophone()
    {
        var allMagnetos = FindObjectsOfType<InteractionScripts.Magnetophone>();
        foreach (var m in allMagnetos)
        {
            currentMagnetophone = m;
            return;
        }

        currentMagnetophone = null;
    }

    IEnumerator WaitForGhostActivation()
    {
        yield return new WaitUntil(() => ghostAI != null && ghostAI.IsAiActive());
        isScriptActive = true;
		Debug.Log($"[AIStoryManager] IA activée");
        yield return StartCoroutine(StartStorySequence());
    }

    IEnumerator StartStorySequence()
    {
        yield return new WaitForSeconds(delayBeforeFirstAction);

		Debug.Log($"[AIStoryManager] 1");
        // Step 1 — activate trunk1, ask question
        photonView.RPC(nameof(RPC_SetTrunk1Unlocked), RpcTarget.AllBuffered);
        yield return new WaitForSeconds(timeBeforeFirstQuestion);

        PlayDialogue(firstQuestion);

        yield return AskPlayerQuestion("yes", "no",
            () => // ✅ yes
            {
                PlayDialogue(responseYes_1);
                photonView.RPC(nameof(RPC_SetPhotoTakeable), RpcTarget.AllBuffered);
            },
            () => // ❌ no
            {
                ghostController.FlickerSwitchLights();
                PlayDialogue(responseNo_1);
                photonView.RPC(nameof(RPC_SetPhotoTakeable), RpcTarget.AllBuffered);
            },
            () => // ⏱️ silence or other
            {
                ghostController.FlickerSwitchLights();
                ghostController.ToggleDoorsRandomly();
                PlayDialogue(responseNo_1);
                photonView.RPC(nameof(RPC_SetPhotoTakeable), RpcTarget.AllBuffered);
            });

        // Step 2 — wait for photo
        yield return new WaitUntil(() => loreManager.IsPhotoTaken());
        PlayDialogue(afterPhotoTaken);
        yield return new WaitForSeconds(afterPhotoTaken.length);

        // Step 3 — ask about regret
        PlayDialogue(secondQuestion);
        yield return AskPlayerQuestion("yes", "no",
            () => // ✅ yes → poser question 3
            {
                PlayDialogue(thirdQuestion);
                StartCoroutine(AskThirdQuestion());
            },
            () => // ❌ no → réaction directe
            {
                PlayDialogue(responseNo_2);
                photonView.RPC(nameof(RPC_SetClothesUnlocked), RpcTarget.AllBuffered);
            },
            () => // ⏱️ silence/autre → même réaction que no
            {
                PlayDialogue(responseNo_2);
                photonView.RPC(nameof(RPC_SetClothesUnlocked), RpcTarget.AllBuffered);
            });

        // Step 4 — wait for diary
        yield return new WaitUntil(() => loreManager.IsDiaryTaken());
        PlayDialogue(afterDiaryTaken);
        yield return new WaitForSeconds(afterDiaryTaken.length);

        // Step 5 — final question
        PlayDialogue(finalQuestion);
        yield return AskPlayerQuestion("yes", "no",
            () => // ✅ yes
            {
                PlayDialogue(finalResponseYes);
                photonView.RPC(nameof(RPC_SetTrunk2Unlocked), RpcTarget.AllBuffered);
            },
            () => // ❌ no
            {
                PlayDialogue(finalResponseNo);
                photonView.RPC(nameof(RPC_SetTrunk2Unlocked), RpcTarget.AllBuffered);
            },
            () => // ⏱️ silence or other
            {
                PlayDialogue(finalResponseNo);
                photonView.RPC(nameof(RPC_SetTrunk2Unlocked), RpcTarget.AllBuffered);
            });

        // Step 6 — wait for radio
        yield return new WaitUntil(() => loreManager.IsRadioPlayed());
        yield return new WaitForSeconds(afterRadioPlayed.length);

        // Final — unlock plank
        if (plank != null) plank.UnlockPlank();
    }

    IEnumerator AskThirdQuestion()
    {
        yield return AskPlayerQuestion("yes", "no",
            () => // ✅ yes
            {
                PlayDialogue(responseYes_3);
                photonView.RPC(nameof(RPC_SetClothesUnlocked), RpcTarget.AllBuffered);
            },
            () => // ❌ no
            {
                PlayDialogue(responseNo_3);
                photonView.RPC(nameof(RPC_SetClothesUnlocked), RpcTarget.AllBuffered);
            },
            () => // ⏱️ silence/autre
            {
                PlayDialogue(responseNo_3);
                photonView.RPC(nameof(RPC_SetClothesUnlocked), RpcTarget.AllBuffered);
            });
    }

    IEnumerator AskPlayerQuestion(string yesKeyword, string noKeyword, Action onYes, Action onNo, Action onTimeout)
	{
    	awaitingResponse = true;
    	bool hasResponded = false;

    	void HandleResponse(string keyword)
    	{
        	if (!awaitingResponse) return;

        	awaitingResponse = false;
        	hasResponded = true;
			isWaitingForAnswer = false;
        	pendingQuestionHandler = null;

        	Debug.Log($"[AIStoryManager] Réponse reçue : {keyword}");

        	if (keyword == yesKeyword)
        	{	
            	onYes?.Invoke();
        	}
        	else if (keyword == noKeyword)
        	{
            	onNo?.Invoke();
        	}
        	else
        	{	
            	onTimeout?.Invoke();
        	}
    	}

    	isWaitingForAnswer = true;
    	pendingQuestionHandler = HandleResponse;

    	TryAssignHandlerToCurrentMagnetophone();

    	float timer = 0f;
    	while (timer < defaultAnswerTimeout && awaitingResponse)
    	{
        	timer += Time.deltaTime;
        	yield return null;
    	}

    	if (!hasResponded && awaitingResponse)
    	{
        	awaitingResponse = false;
        	Debug.Log("[AIStoryManager] Aucun mot reconnu dans le délai imparti. Timeout.");
        	onTimeout?.Invoke();
    	}

    	currentMagnetophone?.ClearQuestionHandler();
	}

	public void TryAssignHandlerToCurrentMagnetophone()
	{
    	if (currentMagnetophone == null || !isWaitingForAnswer || pendingQuestionHandler == null)
        	return;

    	Debug.Log("[AIStoryManager] Tentative d’assignation du handler à un nouveau magneto");

    	if (photonView.IsMine)
    	{
        	currentMagnetophone.SetQuestionHandler(pendingQuestionHandler);
    	}
    	else
    	{
        	currentMagnetophone.photonView.RPC("RPC_ForceSetQuestionHandler", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    	}
	}

    void PlayDialogue(AudioClip clip)
	{
    	if (clip != null)
    	{
        	Debug.Log($"[AIStoryManager] Playing clip: {clip.name}");
        	ghostVoice.PlayOneShot(clip);
    	}
    	else
    	{
        	Debug.LogWarning("[AIStoryManager] Tried to play a null clip.");
    	}
	}

    // 🔁 RPCs pour synchroniser les effets côté clients
    [PunRPC] private void RPC_SetTrunk1Unlocked() => loreManager.SetTrunk1Unlocked(true);
    [PunRPC] private void RPC_SetTrunk2Unlocked() => loreManager.SetTrunk2Unlocked(true);
    [PunRPC] private void RPC_SetPhotoTakeable() => loreManager.SetPhotoTakeable(true);
    [PunRPC] private void RPC_SetClothesUnlocked() => loreManager.SetClothesUnlocked(true);
}
