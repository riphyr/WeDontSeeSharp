using Labyrinthe;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public string sceneToLoad = "ScenePerdu"; // Scène à charger
    public float detectionRange = 20f;
    public float catchDistance = 2f;
    public Message TextMessage;

    private NavMeshAgent agent;
    private Animator animator;

    private Transform playerTarget;  // Transform du joueur (tagué "Player")
    private bool hasLoggedSearch = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerTarget == null)
        {
            TryFindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= catchDistance)
        {
            TeleportPlayer();
        }
        else if (distance <= detectionRange)
        {
            agent.SetDestination(playerTarget.position);
            animator.SetBool("isWalking", true);
        }
        else
        {
            agent.ResetPath();
            animator.SetBool("isWalking", false);
        }
    }

    void TryFindPlayer()
    {
        if (!hasLoggedSearch)
        {
            Debug.Log("Recherche de l'objet avec le tag 'Player'...");
            hasLoggedSearch = true;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Debug.Log(playerObj);
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            Debug.Log("Joueur trouvé ! L’ennemi commence à le suivre.");
            TextMessage.Text();
        }
    }

    void TeleportPlayer()
    {
        Debug.Log("Joueur attrapé. Téléportation vers la scène : " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}