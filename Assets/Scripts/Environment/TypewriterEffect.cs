using System.Collections;
using TMPro;
using UnityEngine;

namespace InteractionScripts
{
    public class TypewriterEffect : MonoBehaviour
    {
        public TMP_Text textMeshPro;
        public float typingSpeed = 0.05f;
        public float textStayDuration = 2f;

        private string fullText;
        private Coroutine typingCoroutine;

        void Awake()
        {
            if (textMeshPro == null)
            {
                textMeshPro = GetComponent<TMP_Text>();
            }
            fullText = textMeshPro.text; 
            Debug.Log($"📜 Texte récupéré au démarrage : {fullText}");
            textMeshPro.text = "";
        }

        public void StartTyping()
        {
            gameObject.SetActive(true); 

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            
            typingCoroutine = StartCoroutine(TypeText());
        }

        private IEnumerator TypeText()
        {
            textMeshPro.text = "";

            foreach (char letter in fullText)
            {
                textMeshPro.text += letter;
                 Debug.Log($"📝 Ajout de la lettre : {letter}");
                textMeshPro.rectTransform.anchoredPosition += new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(textStayDuration);

            gameObject.SetActive(false); 
        }
    }
}
