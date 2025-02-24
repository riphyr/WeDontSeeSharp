using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(CharacterController))]

public class PlayerScript : MonoBehaviour, IPunObservable
{
	[Header("NUMERICAL PARAMETERS")]
	public float walkSpeed = 1.2f;				// Vitesse de marche
	public float runSpeed = 2.0f;				// Vitesse de course
	public float jumpSpeed = 5.0f;				// Vitesse de saut
	public float gravity = 20.0f;				// Vitesse de chute

	[Header("STAMINA VALORS")]
    public float playerStamina = 100.0f;
    private float _maxStamina = 100.0f;

    [Header("STAMINA MODIFIERS")]
	private bool canRun = true;
    public float _staminaDrain = 30f; 			// vitesse de diminution de la stamina
    public float _staminaRegen = 20f; 			// vitesse de régénération de la stamina

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
	private bool animatorIsJumping;
	
	private PhotonView view;					// Composante de Photon pour la différence entre local/server

	private bool canMove = true;				// Booléen de blocage du joueur (à utiliser sous certains cas)

	public GameObject guiDoorMenu;

	CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;

    void Start()
    {
		// Verification des sensibilités de rotation camera
		lookSensitivityX = 2f * PlayerPrefs.GetFloat("sensitivityX", 2f);
		lookSensitivityY = 2f * PlayerPrefs.GetFloat("sensitivityY", 2f);

		// Initialisation des composants réseaux et physiques
        characterController = GetComponent<CharacterController>();
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
	        string interactKey = PlayerPrefs.GetString("Interact", "None");
	        guiDoorMenu.GetComponent<TMP_Text>().text = $"Press \"{interactKey}\" to interact";
	        
            // Vérification si le joueur est autorisé à bouger
        	if (canMove && Cursor.lockState == CursorLockMode.Locked)
        	{
				MovePlayer();																							
				MoveCamera();
        	}
	        else // Sinon Update des paramètres de pause
	        {
		        UdpateSensitivityCamera();
	        }

	        CheckPauseActivation(); // Activation ou désactivation du menu pause

        }
		else
		{
			// Application des paramètres synchronisés pour les autres joueurs
            animator.SetFloat("Sides", animatorSides);
            animator.SetFloat("Front/Back", animatorFrontBack);
			animator.SetBool("isRunning", animatorIsRunning);
			animator.SetBool("isJumping", animatorIsJumping);
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
			stream.SendNext(animatorIsJumping);
        }
        else
        {
            // Reception des paramètres de l'Animator depuis le serveur
            animatorSides = (float)stream.ReceiveNext();
            animatorFrontBack = (float)stream.ReceiveNext();
			animatorIsRunning = (bool)stream.ReceiveNext();
			animatorIsJumping = (bool)stream.ReceiveNext();
			
			// Appliquer ces paramètres à l'Animator local
        	animator.SetFloat("Sides", animatorSides);
        	animator.SetFloat("Front/Back", animatorFrontBack);
			animator.SetBool("isRunning", animatorIsRunning);
			animator.SetBool("isJumping", animatorIsJumping);
        }
    }

	private void CheckPauseActivation()
	{
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
	
	// Update des sensibilités de rotation camera
	private void UdpateSensitivityCamera()
	{
		lookSensitivityX = 2f * PlayerPrefs.GetFloat("sensitivityX", 2f);
		lookSensitivityY = 2f * PlayerPrefs.GetFloat("sensitivityY", 2f);
	}

	// Déplacements de camera
	private void MoveCamera()
	{
		rotationX += -Input.GetAxis("Mouse Y") * lookSensitivityX;												// Detection du mouvement Y de souris
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);											// Blocage du mouvement Y selon les paramètres prédéfinis
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);								// Déplacement de la camera selon les mouvements de la souris
        character.transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSensitivityY, 0);	// Rotation du joueur pour suivre les mouvements camera
	}

	// Déplacements latéraux
    private void MovePlayer()
    {
        // Initialisation du droites vectorielles
        Vector3 direction = Vector3.zero;
		
		// Touches de déplacement
		string forwardKey = PlayerPrefs.GetString("Forward", "None");
    	string backwardKey = PlayerPrefs.GetString("Backward", "None");
    	string leftKey = PlayerPrefs.GetString("Left", "None");
    	string rightKey = PlayerPrefs.GetString("Right", "None");
    	string sprintKey = PlayerPrefs.GetString("Sprint", "None");
    	string jumpKey = PlayerPrefs.GetString("Jump", "None");

		// Detection du changement d'état
		if (Input.GetKey(GetKeyCodeFromString(forwardKey))) 
			direction += transform.forward;
    	if (Input.GetKey(GetKeyCodeFromString(backwardKey))) 
			direction -= transform.forward;
    	if (Input.GetKey(GetKeyCodeFromString(leftKey))) 
			direction -= transform.right;
    	if (Input.GetKey(GetKeyCodeFromString(rightKey))) 
			direction += transform.right;

		// Normalisation du déplacement diagonal
		direction = direction.normalized;

		// Gestion du sprint avec stamina
        bool isRunning = Input.GetKey(GetKeyCodeFromString(sprintKey));
        
        // Gestion de la stamina
        if (!isRunning && playerStamina < _maxStamina)
        {
	        if (playerStamina > 5f) 
		        canRun = true;
	        
	        playerStamina += _staminaRegen * Time.deltaTime;
	        
	        if (playerStamina > _maxStamina) 
		        playerStamina = _maxStamina;
        }
        else if (isRunning && canRun)
        {
            playerStamina -= _staminaDrain * Time.deltaTime;
            if (playerStamina <= 0f)
            {
	            canRun = false;
	            playerStamina = 0f;
            }
        }

        float currentSpeed = canRun && isRunning ? runSpeed : walkSpeed;
        
		// Gestion du saut
        if (characterController.isGrounded)
        {
            moveDirection.y = -1f; // Garde le joueur au sol
            if (Input.GetKeyDown(GetKeyCodeFromString(jumpKey)))
            {
                moveDirection.y = jumpSpeed;
                animator.SetBool("isJumping", true);
            }
            else
            {
                animator.SetBool("isJumping", false);
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime; // Application de la gravité
            animator.SetBool("isJumping", true);
        }
        
        // Application du mouvement horizontal
        Vector3 horizontalMovement = direction * currentSpeed;

		// Fusion avec le mouvement vertical
        Vector3 finalMovement = horizontalMovement + Vector3.up * moveDirection.y;
    
        // Déplacement via CharacterController
        characterController.Move(finalMovement * Time.deltaTime);

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

        // Sprint et état de marche
        bool isRunning = Input.GetKey(GetKeyCodeFromString(PlayerPrefs.GetString("Sprint", "None"))) && canRun;

        // Attribution des valeurs pour l'Animator (int)
        moveY = isMovingForward ? 1 : (isMovingBackward ? -1 : 0);
        moveX = isMovingRight ? 1 : (isMovingLeft ? -1 : 0);

        // Mise à jour de l'Animator avec des entiers
        animator.SetFloat("Sides", moveX);
        animator.SetFloat("Front/Back", moveY);
        animator.SetBool("isRunning", isRunning);

        // Enregistrement pour la synchronisation réseau
        animatorSides = moveX;
        animatorFrontBack = moveY;
        animatorIsRunning = isRunning;
	}
}