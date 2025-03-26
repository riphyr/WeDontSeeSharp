using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Doll : MonoBehaviour
{
    public Text texteEtatJeu; // Texte UI pour afficher l'état du jeu
    public Text texteAction; // Texte UI pour donner l'instruction au joueur

    static System.Random random = new System.Random();
    static bool poupéeRegarde = false; // Indique si la poupée regarde ou pas
    static int positionJoueur = 0; // Position du joueur (sur une ligne)
    static int positionPoupée = 10; // Position de la poupée

    void Start()
    {
        // Initialisation de l'interface
         texteEtatJeu.text = "Bienvenue dans le jeu 'Un, deux, trois soleil' !";
        texteAction.text = "Déplacez-vous lorsque la poupée ne regarde pas !";
        StartCoroutine(JeuCoroutine());
    }

    IEnumerator JeuCoroutine()
    {
        while (true)
        {
            // La poupée commence un cycle de regarder et ne pas regarder
            ChangerRegardPoupée();

            // Afficher l'état du jeu
            AfficherEtatJeu();

            // Attendre un petit délai avant que la poupée change de regard
            yield return new WaitForSeconds(1f); // Pause d'1 seconde

            // Demander au joueur de se déplacer si la poupée ne regarde pas
            if (!poupéeRegarde)
            {
                texteAction.text = "La poupée ne regarde pas ! Appuyez sur 'd' pour avancer.";
                if (Input.GetKeyDown(KeyCode.D))
                {
                    positionJoueur++;
                    texteAction.text = "Vous avez avancé !";
                }
            }
            else
            {
                texteAction.text = "La poupée regarde !";
                if (Input.GetKeyDown(KeyCode.D))
                {
                    texteAction.text = "Vous avez bougé pendant que la poupée regardait ! Vous êtes attrapé !";
                    positionJoueur = 0;  // Recommencer à la position initiale
                }
            }

            // Vérification si le joueur a atteint la poupée
            if (positionJoueur >= positionPoupée)
            {
                texteAction.text = "Félicitations ! Vous avez atteint la poupée !";
                break;
            }

            // Pause entre les tours (avant de recommencer la boucle)
            yield return new WaitForSeconds(1.5f); // Pause de 1.5 seconde
        }

        texteAction.text = "Fin du jeu !";
    }

    // Fonction pour alterner si la poupée regarde ou non
    void ChangerRegardPoupée()
    {
        // 50% de chance que la poupée regarde ou non à chaque cycle
        poupéeRegarde = random.Next(0, 2) == 0;
    }

    // Fonction pour afficher l'état actuel du jeu
    void AfficherEtatJeu()
    {
        texteEtatJeu.text = $"Position du joueur: {positionJoueur}\nPosition de la poupée: {positionPoupée}\nLa poupée {(poupéeRegarde ? "regarde" : "ne regarde pas")}";
    }
}
