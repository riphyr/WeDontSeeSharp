using UnityEngine;

public class OrbitRotation : MonoBehaviour
{
    public float rotationSpeed = 1000f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, -mouseX, Space.World);

            transform.Rotate(Vector3.right, mouseY, Space.Self);
        }
    }
}