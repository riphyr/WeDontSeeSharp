using UnityEngine;

public class PreviewFaceDetector : MonoBehaviour
{
    [Header("Références dynamiques")]
    [SerializeField] private Transform faceToReveal;
    [SerializeField] private ContentEntrance contentEntrance;

    [Header("Options")]
    [SerializeField] private float revealThreshold = 0.6f;

    private bool isShown = false;

    public void SetFace(Transform faceTransform)
    {
        faceToReveal = faceTransform;
    }

    public void ResetPanel()
    {
        isShown = false;
        contentEntrance?.ResetContent();
    }

    void Update()
    {
        if (faceToReveal == null || contentEntrance == null)
            return;

        Vector3 toCamera = (transform.position - faceToReveal.position).normalized;
        float dot = Vector3.Dot(faceToReveal.forward, toCamera);

        if (dot > revealThreshold && !isShown)
        {
            contentEntrance.AppearWithContent();
            isShown = true;
        }
        else if (dot <= revealThreshold && isShown)
        {
            contentEntrance.ResetContent();
            isShown = false;
        }
    }
}