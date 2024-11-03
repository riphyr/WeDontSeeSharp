using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Fonction pour démarrer le jeu
    public void PlayGame()
    {
        // Remplace "GameScene" par le nom de ta scène de jeu
        SceneManager.LoadScene("GameScene");
    }

    // Fonction pour ouvrir les options
    public void OpenOptions()
    {
        // Implémente la logique pour ouvrir les options
        // Par exemple, activer un panneau d'options
        Debug.Log("Options button clicked");
    }

    // Fonction pour quitter le jeu
    public void QuitGame()
    {
        Debug.Log("Quit button clicked");
        Application.Quit();
    }
}