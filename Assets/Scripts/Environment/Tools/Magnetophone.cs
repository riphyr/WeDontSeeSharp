using UnityEngine;
using Photon.Pun;
using System;
using System.Speech.Recognition;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonView))]
    public class Magnetophone : MonoBehaviourPun, IPunObservable
    {
        [Header("References")]
        private AudioSource audioSource;
        public AudioClip beepSound;
        public AudioClip pickupSound;
        private SpeechRecognitionEngine recognizer;
        private float confidenceThreshold = 0.65f;

        [Header("Ghost link")] 
        private GhostAI ghostScript;
        private GhostInteractionController ghostController;
        
        [Header("AI Story Manager")] 
        private Action<string> questionHandler;

		[Header("Ghost Voice Lines")]
		public AudioClip voiceImHere;
		public AudioClip voiceMyName;
		public AudioClip voiceKill;
		public AudioClip voiceNever;
        
        private Transform ownerTransform;
        private Vector3 networkPosition;
        private Quaternion networkRotation;

        private PhotonView view;
        private bool isTaken = false;

        #region Basic Functions
        private void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();

            ghostScript = FindObjectOfType<GhostAI>();
            ghostController = FindObjectOfType<GhostInteractionController>();
            
			var storyManager = FindObjectOfType<AIStoryManager>();
    		if (storyManager != null)
    		{	
        		storyManager.TryAssignHandlerToCurrentMagnetophone();
    		}
        }

        public void PickupMagnetophone(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            ghostScript.AskForActivation();
            ghostScript.RefreshPlayerList();
            inventory.AddItem("Magnetophone");
            photonView.RPC("PlayPickupSound", RpcTarget.All);
        }

        [PunRPC]
        private void PlayPickupSound()
        {
            StartCoroutine(PlaySoundAndDestroy());
        }

        private IEnumerator PlaySoundAndDestroy()
        {
            audioSource.PlayOneShot(pickupSound);
            yield return new WaitForSeconds(pickupSound.length);
            photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
        }

        [PunRPC]
        private void DestroyForAll()
        {
            Destroy(gameObject);
        }

        public void AssignOwner(Photon.Realtime.Player player, Transform newOwnerTransform)
        {
            this.ownerTransform = newOwnerTransform;
            photonView.TransferOwnership(player);

            photonView.RPC("RPC_SetOwnerTransform", RpcTarget.OthersBuffered, player.ActorNumber);
        }

        [PunRPC]
        private void RPC_SetOwnerTransform(int playerID)
        {
            Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerID);
            if (player != null)
            {
                ownerTransform = player.TagObject as Transform;
            }
        }

        public void ActivateMagnetophone()
        {
            if (view.IsMine)
            {
                view.RPC("RPC_PlayBeep", RpcTarget.All);
                StartCoroutine(StartRecognitionWithDelay(1.5f));
            }
        }

        [PunRPC]
        private void RPC_PlayBeep()
        {
            audioSource.PlayOneShot(beepSound, 0.5f);
        }
        

        private IEnumerator StartRecognitionWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartRecognition();
        }
        #endregion

        private void StartRecognition()
        {
            try
            {
                recognizer = new SpeechRecognitionEngine();
                recognizer.SetInputToDefaultAudioDevice();

                // YES synonyms
                Choices yesChoices = new Choices();
                yesChoices.Add(new SemanticResultValue("yes", "yes"));
                yesChoices.Add(new SemanticResultValue("yeah", "yes"));
                yesChoices.Add(new SemanticResultValue("of course", "yes"));
                yesChoices.Add(new SemanticResultValue("obviously", "yes"));

                // NO synonyms
                Choices noChoices = new Choices();
                noChoices.Add(new SemanticResultValue("no", "no"));
                noChoices.Add(new SemanticResultValue("nope", "no"));
                noChoices.Add(new SemanticResultValue("never", "no"));
                noChoices.Add(new SemanticResultValue("not at all", "no"));

                // Other fixed ghost interaction phrases
                Choices ghostCommands = new Choices();
                ghostCommands.Add("are you here");
                ghostCommands.Add("what is your name");
                ghostCommands.Add("i am here to defeat you");
                ghostCommands.Add("leave me alone");
				ghostCommands.Add("kill me");
				ghostCommands.Add("toggle doors");
				ghostCommands.Add("toggle lights");

                // Combine all
                GrammarBuilder gb = new GrammarBuilder();
                gb.Append(new Choices(yesChoices, noChoices, ghostCommands));

                Grammar g = new Grammar(gb);
                recognizer.LoadGrammar(g);

                recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(1);
                recognizer.BabbleTimeout = TimeSpan.FromSeconds(1.5);

                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur reconnaissance vocale : {e.Message}");
            }
        }
        
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < confidenceThreshold)
            {
                Debug.Log($"Faux positif ignoré : {e.Result.Text} (Confiance : {e.Result.Confidence * 100:F1}%)");
                return;
            }

            string semantic = e.Result.Semantics?.Value?.ToString().ToLowerInvariant() ?? e.Result.Text.ToLowerInvariant();
            Debug.Log($"Détection confirmée : {e.Result.Text} → {semantic} ({e.Result.Confidence * 100:F1}%)");

            view.RPC("RPC_SendAnswerToHost", RpcTarget.MasterClient, semantic);

            // Sinon : fallback RPC pour les ghost phrases
            view.RPC("RPC_HandleRecognition", RpcTarget.All, e.Result.Text.ToLowerInvariant());
        }


        [PunRPC]
		private void RPC_HandleRecognition(string recognizedText)
		{
			switch (recognizedText)
 			{
        		case "are you here":
            		view.RPC("RPC_PlayGhostVoice", RpcTarget.All, "im_here");
            		ghostController.FlickerSwitchLights();
            		break;

        		case "what is your name":
            		view.RPC("RPC_PlayGhostVoice", RpcTarget.All, "my_name");
            		break;

        		case "i am here to defeat you":
            		view.RPC("RPC_PlayGhostVoice", RpcTarget.All, "kill_you");
            		ghostController.IncreaseAggressivity();
            		break;

        		case "leave me alone":
            		view.RPC("RPC_PlayGhostVoice", RpcTarget.All, "never");
            		ghostController.ToggleDoorsRandomly();
            		break;

        		case "kill me":
            		ghostController.IncreaseAggressivity();
            		ghostController.FlickerFlashlights();
            		ghostController.ToggleDoorsRandomly();
            		break;

				case "toggle doors":
            		ghostController.ToggleDoorsRandomly();
            		break;

				case "toggle lights":
            		ghostController.FlickerFlashlights();
            		ghostController.FlickerSwitchLights();
            		break;
    		}
		}

		[PunRPC]
		private void RPC_PlayGhostVoice(string clipId)
		{
    		switch (clipId)
    		{
        		case "im_here":
           			audioSource.PlayOneShot(voiceImHere, 0.3f);
            		break;
        		case "my_name":
            		audioSource.PlayOneShot(voiceMyName, 0.3f);
            		break;
        		case "kill_you":
            		audioSource.PlayOneShot(voiceKill, 0.3f);
            		break;
        		case "never":
            		audioSource.PlayOneShot(voiceNever, 0.3f);
            		break;
    		}
		}

        [PunRPC]
        private void RPC_SendAnswerToHost(string semanticKeyword)
        {
            if (questionHandler != null)
            {
                questionHandler.Invoke(semanticKeyword);
            }
			else
				Debug.Log("Nulle question HAndler");
        }

		[PunRPC]
		private void RPC_ForceSetQuestionHandler(int actorId)
		{
    		if (!photonView.IsMine) return;

    		if (PhotonNetwork.LocalPlayer.ActorNumber == actorId)
    		{
        		Debug.Log("[Magnetophone] Réception RPC pour enregistrer questionHandler du joueur");
        		questionHandler = (semantic) =>
        		{
            		photonView.RPC("RPC_SendAnswerToHost", RpcTarget.MasterClient, semantic);
        		};
   		 	}
		}

        public void SetQuestionHandler(Action<string> handler)
        {
			Debug.Log("[Magnetophone] Question handler enregistré.");
            questionHandler = handler;
        }

        public void ClearQuestionHandler()
        {
			Debug.Log("[Magnetophone] Question handler clear.");
            questionHandler = null;
        }

        #region Functions for Destroy/Update/Spawn
        private void OnDestroy()
        {
            if (recognizer != null)
            {
                recognizer.Dispose();
            }
        }

        void Update()
        {
            if (photonView.IsMine)
            {
                if (ownerTransform != null)
                {
                    transform.position = ownerTransform.position + ownerTransform.forward * 0.3f + ownerTransform.right * 0.1f + Vector3.up * 0.3f;
                    transform.rotation = Quaternion.Euler(0f, ownerTransform.eulerAngles.y - 180f, 0f);
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10);
            }

			if (isTaken)
            {
                isTaken = false;
                ActivateMagnetophone();
            }
        }

        public void ShowMagnetophone(bool show)
        {
            isTaken = show;

            if (!show)
            {
                photonView.RPC("DestroyForAll", RpcTarget.AllBuffered);
            }
        }

         public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
            }
        }
        #endregion
    }
}
