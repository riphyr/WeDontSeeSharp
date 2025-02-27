using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 2f; // Sensibilité de la souris
    private float verticalRotation = 0f; // Rotation verticale (haut-bas)
    private Vector3 initialPosition; // Position initiale sauvegardée
    private Quaternion initialRotation; // Rotation initiale sauvegardée

    void Start()
    {
        // Sauvegarder la position et la rotation initiales définies dans l'inspecteur
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        Debug.Log($"initialRotation: {initialRotation}");

        // Verrouiller le curseur au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;

        // Optionnel : Restaurer les coordonnées initiales au démarrage (si nécessaire)
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    void Update()
    {
        // Gérer les mouvements de la souris
        HandleMouseLook();
    }

    void HandleMouseLook()
    {
        // Récupérer les mouvements de la souris
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; // Mouvement horizontal
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; // Mouvement vertical

        // Gérer la rotation verticale (limité pour éviter une rotation complète)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Limite entre -90° et 90°

        // Appliquer la rotation verticale sur la caméra
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Appliquer la rotation horizontale (autour de l'axe Y)
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}




/*
 using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Camera playerCamera; // Variable pour la caméra
    public float mouseSensitivity = 2f; // Sensibilité de la souris
    private float verticalRotation = 0f; // Rotation verticale (haut-bas)
    public float rotationSpeed = 100f; // Vitesse de rotation (si utilisé pour le parent)

    void Start()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Veuillez assigner une caméra dans le champ 'Player Camera' de l'Inspector.");
            enabled = false; // Désactive le script s'il manque la caméra
            return;
        }

        // Verrouiller le curseur au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Récupérer les mouvements de la souris
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; // Mouvement vertical
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; // Mouvement horizontal

        // Gérer la rotation verticale (limité pour éviter une rotation à 360°)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Limite entre -90° et 90°

        // Appliquer la rotation sur la caméra
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Gérer la rotation horizontale (affecte le parent si configuré)
        if (transform.parent != null)
        {
            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }
}



using UnityEngine;

public class MouseCameraRotation : MonoBehaviour
{
    public float mouseSensitivity = 100f; // Sensibilité de la souris
    public Transform playerBody;         // Référence au joueur (pour la rotation horizontale)

    private float xRotation = 0f;        // Stocke la rotation verticale

    void Start()
    {
        // Verrouille le curseur au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Collecte les mouvements de la souris
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotation verticale (limite pour éviter un retournement complet)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limite entre -90° et 90°

        // Appliquer la rotation verticale
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotation horizontale (tourne le joueur entier)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}


using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 2f; // Sensibilité de la souris
    private float verticalRotation = 0f; // Rotation verticale (haut-bas)
    public float rotationSpeed = 100f;

    void Start()
    {
        // Verrouiller le curseur au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Récupérer les mouvements de la souris
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; // Mouvement vertical
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; // Mouvement horizontal

        // Gérer la rotation verticale (limité pour éviter une rotation à 360°)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Limite entre -90° et 90°

        // Appliquer la rotation sur la caméra
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Gérer la rotation horizontale (affecte uniquement la caméra)
        //transform.parent.Rotate(Vector3.up * mouseX); // Le parent de la caméra pivote autour de l'axe Y
        // Rotation autour de l'axe vertical (Y)
        
    }
}*/