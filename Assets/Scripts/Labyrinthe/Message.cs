using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;  // Pour le texte UI

namespace Labyrinthe
{
    public class Message : MonoBehaviour
    {
        public TextMeshProUGUI instructionText; // Le texte explicatif à afficher



        // Méthode pour afficher les instructions et démarrer le timer

        public void Text()
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "Find the card before it finds you...";
            StartCoroutine(HideInstructionTextAfterDelay(7f));
        }

        // Coroutine pour masquer le texte explicatif après 10 secondes
        private IEnumerator HideInstructionTextAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay); // Attendre 10 secondes
            instructionText.text = ""; // Masquer le texte explicatif
            instructionText.gameObject.SetActive(false);
        }


    }
}