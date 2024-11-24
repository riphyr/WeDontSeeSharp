using UnityEngine;

public class LockScale : MonoBehaviour
{
    private Vector3 initialScale;

    void Awake()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = initialScale;
    }
}