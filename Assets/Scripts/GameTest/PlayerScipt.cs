using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerScript : MonoBehaviour, IPunObservable
{
	[Header("NUMERICAL PARAMETERS")]
	public float walkSpeed = 1.5f;				// Vitesse de marche
	public float runSpeed = 3f;					// Vitesse de course

	[Header("STAMINA VALORS")]
    public float playerStamina = 100.0f;
    private float _maxStamina = 100.0f;

    [Header("STAMINA MODIFIERS")]
	private bool canRun = true;
    private float _staminaDrain = 15f; 		// vitesse de diminution de la stamina
    private float _staminaRegen = 12f; 		// vitesse de régénération de la stamina

    [Header("LOOKING PARAMETERS")]
	public float lookSensitivityX;    			// Vitesse de rotation de la caméra sur l'axe X
	public float lookSensitivityY;    			// Vitesse de rotation de la caméra sur l'axe Y
    public float lookXLimit = 45f;  			// Limite de rotation verticale de la caméra
	private float rotationX = 0;				// Initialisation de la rotation en X pour la caméra

	[Header("ELEMENTS")]
	public Camera playerCamera;					// GameObject relié à la caméra
	public GameObject character;				// GameObject relié au prefab du joueur
	public Animator animator;					// Controller pour les animations

	[Header("MENUS")]
	public GameObject pauseObject;				// Menu de pause

	// Synchronisation des animations
    private float animatorSides;
    private float animatorFrontBack;
	private bool animatorIsRunning;
	
    private Rigidbody rb;						// Composante de collision lié au moteur de jeu
	private PhotonView view;					// Composante de Photon pour la différence entre local/server

	private bool canMove = true;				// Booléen de blocage du joueur (à utiliser sous certains cas)

	public GameObject speed;

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
		string staminaText = playerStamina.ToString();
		speed.GetComponent<TMP_Text>().text = staminaText;

		// Test s'appliquant uniquement pour le joueur local
        if (view.IsMine)
        {		
            // Vérification si le joueur est autorisé à bouger
        	if (canMove && Cursor.lockState == CursorLockMode.Locked)
        	{
				MovePlayer();																							// Focntion de saut conditionné au sol
				
            	rotationX += -Input.GetAxis("Mouse Y") * lookSensitivityX;												// Detection du mouvement Y de souris
            	rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);											// Blocage du mouvement Y selon les paramètres prédéfinis
            	playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);								// Déplacement de la camera selon les mouvements de la souris
            	character.transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSensitivityY, 0);	// Rotation du joueur pour suivre les mouvements camera
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
		else
		{
			// Application des paramètres synchronisés pour les autres joueurs
            animator.SetFloat("Sides", animatorSides);
            animator.SetFloat("Front/Back", animatorFrontBack);
			animator.SetBool("isRunning", animatorIsRunning);
		}

    }

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Envoie des paramètres de l'Animator locaux aux autres joueurs
			stream.SendNext(animatorSides);
            stream.SendNext(animatorFrontBack);
			stream.SendNext(animatorIsRunning);
        }
        else
        {
            // Reception des paramètres de l'Animator depuis le serveur
            animatorSides = (float)stream.ReceiveNext();
            animatorFrontBack = (float)stream.ReceiveNext();
			animatorIsRunning = (bool)stream.ReceiveNext();
			
			// Appliquer ces paramètres à l'Animator local
        	animator.SetFloat("Sides", animatorSides);
        	animator.SetFloat("Front/Back", animatorFrontBack);
			animator.SetBool("isRunning", animatorIsRunning);
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

		// Neutraliser la composante Y pour empecher de fly sur les murs
    	moveDirection.y = 0;

		// Detection du changement d'état pour la variable de course
		string sprintKey = PlayerPrefs.GetString("Sprint", "None");
		bool isRunning = Input.GetKey(GetKeyCodeFromString(sprintKey));

		//Regeneration stamina
		if (!isRunning && playerStamina < _maxStamina)
        {
			if (playerStamina > 5f)
				canRun = true;

			playerStamina += _staminaRegen * Time.deltaTime;

            if (playerStamina >= _maxStamina)
            {
				playerStamina = _maxStamina;
            }
        }

		//Diminution stamina
		if(isRunning && canRun)
        {
            playerStamina -= _staminaDrain * Time.deltaTime;

            if (playerStamina <= 0)
            {
                canRun = false;
            }
        }

		// Attribution de la vitesse selon course/marche
		float currentSpeed = (isRunning && canRun && playerStamina >= 5.0f) ? runSpeed : walkSpeed;

        // Déplacement du joueur selon les entrées
        //transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);
		rb.MovePosition(rb.position + moveDirection.normalized * currentSpeed * Time.fixedDeltaTime);

		//Update des animations
		UpdateAnimatorParameters();
    }

	// Fonction d'extraction de touche
	private KeyCode GetKeyCodeFromString(string key)
	{
    	return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
	}

	private void UpdateAnimatorParameters()
	{
    	// Initialisation
    	int moveX = 0;
    	int moveY = 0;

    	// Vérification des touches directionnelles
    	bool isMovingForward = Input.GetKey(GetKeyCodeFromString(PlayerPrefs.GetString("Forward", "None")));
    	bool isMovingBackward = Input.GetKey(GetKeyCodeFromString(PlayerPrefs.GetString("Backward", "None")));
    	bool isMovingLeft = Input.GetKey(GetKeyCodeFromString(PlayerPrefs.GetString("Left", "None")));
    	bool isMovingRight = Input.GetKey(GetKeyCodeFromString(PlayerPrefs.GetString("Right", "None")));

    	// Détection du sprint
    	bool isCurrentlyRunning = Input.GetKey(GetKeyCodeFromString(PlayerPrefs.GetString("Sprint", "None"))) && canRun;

    	// Calcul de la valeur directionnelle
    	if (isMovingForward)
    	{
        moveY =  1;
    	}
    	else if (isMovingBackward)
    	{
        	moveY = -1;
    	}

    	if (isMovingLeft)
    	{
        	moveX = -1;
    	}
    	else if (isMovingRight)
    	{
        	moveX = 1;
    	}

    	// Envoie les valeurs à l'animation animator pour gérer les transitions
    	animator.SetFloat("Sides", moveX); // Déplacement latéral
    	animator.SetFloat("Front/Back", moveY); // Déplacement avant/arrière
    	animator.SetBool("isRunning", isCurrentlyRunning); // Détection du sprint

		// MAJ des variables pour synchroniser les mouvements réseaux avec les autres joueurs
        animatorSides = moveX;
        animatorFrontBack = moveY;
	}
}