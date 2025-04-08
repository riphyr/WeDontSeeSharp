using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;  // Pour le texte UI

namespace Studio
{
    public class Consigne : MonoBehaviour
    {
        public TextMeshProUGUI instructionText;  // Le texte explicatif à afficher
        public TextMeshProUGUI timerText;        // Le texte pour afficher le compte à rebours
        private float timeLimit = 600f;  // 10 minutes en secondes

        // Méthode pour afficher les instructions et démarrer le timer

        public void Text()
        {
            instructionText.gameObject.SetActive(true);
            timerText.gameObject.SetActive(true);
            instructionText.text = "You have 10 minutes! to find all the cards inside the apartment.";
            // Lancer la minuterie
            StartCoroutine(HideInstructionTextAfterDelay(7f));
            StartCoroutine(StartTimer());
        }
        
        // Coroutine pour masquer le texte explicatif après 10 secondes
        private IEnumerator HideInstructionTextAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);  // Attendre 10 secondes
            instructionText.text = "";  // Masquer le texte explicatif
            instructionText.gameObject.SetActive(false);
        }

        // Coroutine pour gérer le compte à rebours
        private IEnumerator StartTimer()
        {
            float currentTime = timeLimit;

            while (currentTime > 0)
            {
                // Afficher le temps restant
                timerText.text = "Time left: " + Mathf.FloorToInt(currentTime / 60) + "m " + Mathf.FloorToInt(currentTime % 60) + "s";
                currentTime -= Time.deltaTime;  // Décrémenter le temps en fonction du deltaTime
                yield return null;
            }

            // Une fois le temps écoulé, masquer le texte explicatif
            //instructionText.text = "";
            timerText.text = "Times up!"; // Afficher un message à la fin du compte à rebours

            // Code pour ce qui se passe après la fin du temps (par exemple, verrouiller la porte ou enchaîner les événements)
            HandleEndOfGame();
        }

        // Cette fonction gère ce qui se passe à la fin du compte à rebours
        private void HandleEndOfGame()
        {
            // Tu peux ici ajouter du code pour verrouiller la porte, empêcher les joueurs de continuer, etc.
            Debug.Log("Le temps est écoulé !");
        }
    }
}