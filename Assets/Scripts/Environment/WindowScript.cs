using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace WindowScript
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PhotonView))]
    public class Window : MonoBehaviour, IPunObservable
    {
        public Animator openAndCloseWindow;
        public bool open = false;
        public AudioSource asource;
        public AudioClip openWindow,closeWindow;
        private PhotonView view;

        void Start()
        {
            asource = GetComponent<AudioSource> ();
            view = GetComponent<PhotonView>();
            if (!openAndCloseWindow)
                openAndCloseWindow = GetComponent<Animator>();
        }

        public void ToggleWindow()
        {
            if (view.IsMine)
            {
                if (!open)
                    StartCoroutine(Opening());
                else
                    StartCoroutine(Closing());
            }
        }

        private IEnumerator Opening()
        {
            Debug.Log("Fenêtre en cours d'ouverture");
            openAndCloseWindow.Play("Openingwindow");
            open = true;
            asource.clip = openWindow;
            asource.Play ();
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator Closing()
        {
            Debug.Log("Fenêtre en cours de fermeture");
            openAndCloseWindow.Play("Closingwindow");
            open = false;
            asource.clip = closeWindow;
            asource.Play ();
            yield return new WaitForSeconds(0.5f);
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(open);
            }
            else
            {
                bool previousState = open;
                open = (bool)stream.ReceiveNext();

                if (open != previousState)
                {
                    if (open)
                        StartCoroutine(Opening());
                    else
                        StartCoroutine(Closing());
                }
            }
        }
    }
}