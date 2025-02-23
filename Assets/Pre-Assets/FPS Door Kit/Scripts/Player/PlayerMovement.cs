
/* ~ FPS Door Kit V1.0 ~ */

using System.Collections;
using UnityEngine;

namespace EnivStudios
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Player Movement")]
        [SerializeField] float movementSpeed;
        [SerializeField] float movementSpeedSmoothTime = 0.08f;

        [Header("Mouse Look")]
        [SerializeField] float mouseLookSensitivity;
        [SerializeField] float mouseLookSmoothTime = 0.01f;

        [Header("Player Jump")]
        [SerializeField] float jumpSpeed;
        [SerializeField] float gravity = -13;
        [SerializeField] AnimationCurve jumpFall;
        CharacterController characterController;
        float cameraPitch = 0;
        float velocityY = 0;
        Camera _cam;
        bool isJumping;
        Vector2 currentDir = Vector2.zero;
        Vector2 currentDirVelocity = Vector2.zero;
        Vector2 currentMouseDelta = Vector2.zero;
        Vector2 currentMouseDeltaVelocity = Vector2.zero;
        private void Awake()
        {
            _cam = GetComponentInChildren<Camera>();
            characterController = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        void Update()
        {
            Move();
            CameraRotation();
        }
        private void Move()
        {
            Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            targetDir.Normalize();

            currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, movementSpeedSmoothTime);
            if (characterController.isGrounded) { velocityY = 0; }
            velocityY += gravity * Time.deltaTime;

            Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * movementSpeed + Vector3.up * velocityY;
            characterController.Move(velocity * Time.deltaTime);

            Jump();
        }
        public void CameraRotation()
        {
            if (Time.timeScale != 0)
            {
                Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseLookSmoothTime);
                cameraPitch -= currentMouseDelta.y * mouseLookSensitivity;
                cameraPitch = Mathf.Clamp(cameraPitch, -90, 90);
                _cam.transform.localEulerAngles = Vector3.right * cameraPitch;
                transform.Rotate(currentMouseDelta.x * mouseLookSensitivity * Vector3.up);
            }
        }
        void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                isJumping = true;
                StartCoroutine(JumpEvent());
            }
        }
        IEnumerator JumpEvent()
        {
            characterController.slopeLimit = 90;
            float timeInAir = 0;
            do
            {
                float jumpForce = jumpFall.Evaluate(timeInAir);
                characterController.Move(jumpForce * jumpSpeed * Time.deltaTime * Vector3.up);
                timeInAir += Time.deltaTime;
                yield return null;
            } while (!characterController.isGrounded && characterController.collisionFlags != CollisionFlags.Above);
            characterController.slopeLimit = 45;
            isJumping = false;
        }
    }
}
