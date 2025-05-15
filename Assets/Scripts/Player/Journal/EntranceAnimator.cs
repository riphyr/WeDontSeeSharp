using UnityEngine;

public class EntranceAnimator : MonoBehaviour
{
    [Header("Animation")]
    public Transform targetTransform;
    public float slideDistanceY = 55f;
    public float slideDuration = 0.5f;

    private Vector3 initialLocalPosition;
    private bool hasSlidIn = false;
    private bool hasInitialized = false;

    public void SlideIn()
    {
        if (targetTransform == null || hasSlidIn) return;

        if (!hasInitialized)
        {
            initialLocalPosition = targetTransform.localPosition;
            hasInitialized = true;
        }

        hasSlidIn = true;
        StartCoroutine(Slide());
    }

    public void ResetSlide()
    {
        hasSlidIn = false;

        if (targetTransform != null && hasInitialized)
            targetTransform.localPosition = initialLocalPosition;
    }

    private System.Collections.IEnumerator Slide()
    {
        Vector3 startPos = initialLocalPosition;
        Vector3 endPos = initialLocalPosition + new Vector3(0f, slideDistanceY, 0f);

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / slideDuration);
            targetTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        targetTransform.localPosition = endPos;
    }
}