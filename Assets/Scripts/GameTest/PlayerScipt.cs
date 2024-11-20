using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerScript : MonoBehaviour
{
	[Header("NUMERICAL PARAMETERS")]
	public float walkSpeed = 5f;				// Vitesse de marche
	public float runSpeed = 10f;				// Vitesse de course
    public float jumpForce = 4.5f;    			// Force de saut
    public float groundCheckRadius = 0.5f;  	// Rayon pour vérifier la présence du sol

    [Header("LOOKING PARAMETERS")]
	public float lookSensitivityX;    			// Vitesse de rotation de la caméra sur l'axe X
	public float lookSensitivityY;    			// Vitesse de rotation de la caméra sur l'axe Y
    public float lookXLimit = 45f;  			// Limite de rotation verticale de la caméra
	private float rotationX = 0;				// Initialisation de la rotation en X pour la caméra

	[Header("ELEMENTS")]
	public Camera playerCamera;					// GameObject relié à la caméra
	public GameObject character;				// GameObject relié au prefab du joueur

	[Header("MENUS")]
	public GameObject pauseObject;				// Menu de pause
	
    private Rigidbody rb;						// Composante de collision lié au moteur de jeu
	private PhotonView view;					// Composante de Photon pour la différence entre local/server

    private bool isGrounded;					// Booléen de détection si le joueur est au sol
	private bool canMove = true;				// Booléen de blocage du joueur (à utiliser sous certains cas)

    void Start()
    {
		// Verification des sensibilités de rotation camera
		lookSensitivityX = 2f * PlayerPrefs.GetFloat("sensitivityX", 2f);
		lookSensitivityY = 2f * PlayerPrefs.GetFloat("sensitivityY", 2f);

		// Initialisation des composants réseaux et physiques
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();

		// Blocage et disparition du curseur
		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        pauseObject.SetActive(false);
    }

    void Update()
    {
		// Test s'appliquant uniquement pour le joueur local
        if (view.IsMine)
        {
            // Vérification si le joueur est autorisé à bouger
        	if (canMove && Cursor.lockState == CursorLockMode.Locked)
        	{
				MovePlayer();																							// Fonction de déplacements latéraux
            	if (isGrounded) Jump();																					// Focntion de saut conditionné au sol
				
            	rotationX += -Input.GetAxis("Mouse Y") * lookSensitivityX;												// Detection du mouvement Y de souris
            	rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);											// Blocage du mouvement Y selon les paramètres prédéfinis
            	playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);								// Déplacement de la camera selon les mouvements de la souris
            	character.transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSensitivityY, 0);		// Rotation du joueur pour suivre les mouvements camera
        	}
	        else
	        {
		        // Update des sensibilités de rotation camera
		        lookSensitivityX = 2f * PlayerPrefs.GetFloat("sensitivityX", 2f);
		        lookSensitivityY = 2f * PlayerPrefs.GetFloat("sensitivityY", 2f);
	        }

	        if (Input.GetKey(GetKeyCodeFromString(PlayerPrefs.GetString("Pause", "None"))))
	        {
		        pauseObject.SetActive(true);
		        Cursor.lockState = CursorLockMode.None;
		        Cursor.visible = true;
	        }
	        
	        if (Cursor.lockState == CursorLockMode.None && Cursor.visible)
	        {
		        pauseObject.SetActive(true);
	        }
	        else
	        {
		        pauseObject.SetActive(false);
	        }
        }

    }

	// Déplacements latéraux
    private void MovePlayer()
    {
        // Initialisation du vecteur moveDirection
        Vector3 moveDirection = Vector3.zero;
		
		// Déplacement avant
		string forwardKey = PlayerPrefs.GetString("Forward", "None");
        if (Input.GetKey(GetKeyCodeFromString(forwardKey)))
            moveDirection += transform.forward;

		// Déplacement arrière
		string backwardKey = PlayerPrefs.GetString("Backward", "None");
        if (Input.GetKey(GetKeyCodeFromString(backwardKey)))
            moveDirection += -transform.forward;

		// Déplacement gauche
		string leftKey = PlayerPrefs.GetString("Left", "None");
        if (Input.GetKey(GetKeyCodeFromString(leftKey)))
            moveDirection += -transform.right;

		// Déplacement droite
		string rightKey = PlayerPrefs.GetString("Right", "None");
        if (Input.GetKey(GetKeyCodeFromString(rightKey)))
            moveDirection += transform.right;

		// Detection du changement d'état pour la variable de course
		string sprintKey = PlayerPrefs.GetString("Sprint", "None");
		bool isRunning = Input.GetKey(GetKeyCodeFromString(sprintKey));

		// Attribution de la vitesse selon course/marche
		float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Déplacement du joueur selon les entrées
        transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);
    }

	// Fonction de saut
    private void Jump()
    {
        // Vérifie si la touche Space est appuyée
		string jumpKey = PlayerPrefs.GetString("Jump", "None");
        if (Input.GetKeyDown(GetKeyCodeFromString(jumpKey)))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);			// Force appliquée vers le haut
            isGrounded = false;  											// Désactivation temporaire de la capacité de saut
        }
    }

	// Fonction d'extraction de touche
	private KeyCode GetKeyCodeFromString(string key)
	{
    	return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
	}

    private void FixedUpdate()
    {
        // Check constant si le joueur est au sol
        isGrounded = Physics.SphereCast(transform.position, groundCheckRadius, Vector3.down, out _, 1f);
    }
}