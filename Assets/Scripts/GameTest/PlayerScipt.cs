using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 5f;    // Vitesse de déplacement
    public float jumpForce = 5f;    // Force de saut
    public float groundCheckRadius = 0.5f;  // Rayon pour vérifier le sol

    private Rigidbody rb;
    private bool isGrounded;

    private PhotonView view;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (view.IsMine)
        {
            MovePlayer();
            if (isGrounded) Jump();
        }
    }

    private void MovePlayer()
    {
        // Déplacement horizontal (gauche et droite) et vertical (avant et arrière)
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDirection += Vector3.forward;  // Avance

        if (Input.GetKey(KeyCode.S))
            moveDirection += Vector3.back;     // Recule

        if (Input.GetKey(KeyCode.A))
            moveDirection += Vector3.left;     // Gauche

        if (Input.GetKey(KeyCode.D))
            moveDirection += Vector3.right;    // Droite

        // Appliquer le mouvement avec Time.deltaTime pour un déplacement fluide
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    private void Jump()
    {
        // Vérifie si la touche Space est appuyée
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;  // Désactiver le saut jusqu'à ce que le joueur touche à nouveau le sol
        }
    }

    private void FixedUpdate()
    {
        // Utilise un SphereCast pour détecter le sol
        isGrounded = Physics.SphereCast(transform.position, groundCheckRadius, Vector3.down, out _, 1f);
    }
}
