using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonTransformView))]
[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    public float walkSpeed = 5f;    // Vitesse de déplacement
    public float runSpeed = 12f;    // Vitesse de course
    public float jumpPower = 5f;    // Force de saut
    public float gravity = 100f;     // Gravité
    public float lookSpeed = 2f;    // Vitesse de rotation de la caméra
    public float lookXLimit = 45f;  // Limite de rotation verticale de la caméra
    public float defaultHeight = 2f; // Hauteur normale du personnage
    public float crouchHeight = 1f; // Hauteur du personnage quand il est accroupi
    public float crouchSpeed = 3f;  // Vitesse de déplacement quand accroupi
    public float groundCheckRadius = 0.5f;  // Rayon pour vérifier le sol
    public KeyCode moveForwardKey = KeyCode.W;  // Touche pour avancer
    public KeyCode moveBackwardKey = KeyCode.S; // Touche pour reculer
    public KeyCode moveRightKey = KeyCode.D;   // Touche pour aller à droite
    public KeyCode moveLeftKey = KeyCode.A;    // Touche pour aller à gauche
    public KeyCode jumpKey = KeyCode.Space;

    private PhotonView view;
    public Camera playerCamera;

    private bool canMove = true;
    private bool isGrounded;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    void Start()
    {
		view = GetComponent<PhotonView>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (view.IsMine)
        {
            playerCamera.gameObject.SetActive(true);
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
		if (!view.IsMine) return;

        // Vérification si le joueur est au sol avec un rayon pour plus de fiabilité
        isGrounded = Physics.SphereCast(transform.position, groundCheckRadius, Vector3.down, out _, 0.1f);

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * (Input.GetKey(moveForwardKey) ? 1 : (Input.GetKey(moveBackwardKey) ? -1 : 0)) : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * (Input.GetKey(moveRightKey) ? 1 : (Input.GetKey(moveLeftKey) ? -1 : 0)) : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Saut : déclenchement uniquement si le joueur est au sol
        if (Input.GetKeyDown(jumpKey) && canMove && isGrounded)
        {
            moveDirection.y = jumpPower;  // Appliquer la force de saut
        }

        // Appliquer la gravité
        if (!isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;  // Appliquer la gravité en l'air
        }
        else
        {
            // Si le joueur est au sol, on empêche le flottement et on réinitialise la direction verticale
            if (moveDirection.y < 0)
            {
                moveDirection.y = -0.5f;  // Une petite valeur pour maintenir le joueur sur le sol
            }
        }

        // Accroupir
        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = Mathf.Lerp(characterController.height, crouchHeight, Time.deltaTime * 10f);
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = Mathf.Lerp(characterController.height, defaultHeight, Time.deltaTime * 10f);
            walkSpeed = 6f;
            runSpeed = 12f;
        }

		if (characterController.height == crouchHeight)
		{
    		Vector3 position = transform.position;
    		position.y = Mathf.Max(position.y, 0f); // Ajuster la position pour ne pas rentrer dans le sol
    		transform.position = position;
		}

        // Appliquer le mouvement
        characterController.Move(moveDirection * Time.deltaTime);

        // Rotation de la caméra et du joueur
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}