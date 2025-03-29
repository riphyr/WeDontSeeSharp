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
        private AudioSource audioSource;
        public AudioClip beepSound;
        public AudioClip pickupSound;
        private SpeechRecognitionEngine recognizer;
        private float confidenceThreshold = 0.65f;

        private Transform ownerTransform;
        private Vector3 networkPosition;
        private Quaternion networkRotation;

        private PhotonView view;
        private bool isTaken = false;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();
        }

        public void PickupMagnetophone(PlayerInventory inventory)
        {
            if (!view.IsMine)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

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

        private void StartRecognition()
        {
            try
            {
                recognizer = new SpeechRecognitionEngine();
                recognizer.SetInputToDefaultAudioDevice();

                Choices commands = new Choices();
                commands.Add(new string[]
                {
                    "are you here",
                    "what is your name",
                    "i am here to defeat you",
                    "leave me alone"
                });

                GrammarBuilder gb = new GrammarBuilder();
                gb.Append(commands);
                Grammar g = new Grammar(gb);
                recognizer.LoadGrammar(g);

                recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(1);
                recognizer.BabbleTimeout = TimeSpan.FromSeconds(1.5);

                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception e)
            {
                Debug.LogError($"🚨 Erreur reconnaissance vocale : {e.Message}");
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < confidenceThreshold)
            {
                Debug.Log($"❌ Faux positif ignoré : {e.Result.Text} (Confiance : {e.Result.Confidence * 100:F1}%)");
                return;
            }

            Debug.Log($"✅ Détection confirmée : {e.Result.Text} ({e.Result.Confidence * 100:F1}%)");

            view.RPC("RPC_HandleRecognition", RpcTarget.All, e.Result.Text);
        }

        [PunRPC]
        private void RPC_HandleRecognition(string recognizedText)
        {
            switch (recognizedText)
            {
                case "are you here":
                    view.RPC("RPC_PlayBeep", RpcTarget.All);
                    Debug.Log("👻 Esprit détecté : 'Yes...'");
                    break;
                case "what is your name":
                    view.RPC("RPC_PlayBeep", RpcTarget.All);
                    Debug.Log("👻 Esprit détecté : 'I'm Fabrice...'");
                    break;
                case "i am here to defeat you":
                    view.RPC("RPC_PlayBeep", RpcTarget.All);
                    Debug.Log("👻 Esprit détecté : 'I will do it first!'");
                    break;
                case "leave me alone":
                    view.RPC("RPC_PlayBeep", RpcTarget.All);
                    Debug.Log("👻 Esprit détecté : 'Never!'");
                    break;
            }
        }

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
    }
}
