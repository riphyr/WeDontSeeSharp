using System.Collections;
using UnityEngine;

namespace WindowScript
{
    [RequireComponent(typeof(Animator))]
    public class Window : MonoBehaviour
    {
        public Animator openAndCloseWindow;
        public bool open = false;

        void Start()
        {
            if (!openAndCloseWindow)
                openAndCloseWindow = GetComponent<Animator>();
        }

        public void ToggleWindow()
        {
            if (!open)
                StartCoroutine(Opening());
            else
                StartCoroutine(Closing());
        }

        private IEnumerator Opening()
        {
            Debug.Log("Fenêtre en cours d'ouverture");
            openAndCloseWindow.Play("Openingwindow");
            open = true;
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator Closing()
        {
            Debug.Log("Fenêtre en cours de fermeture");
            openAndCloseWindow.Play("Closingwindow");
            open = false;
            yield return new WaitForSeconds(0.5f);
        }
    }
}