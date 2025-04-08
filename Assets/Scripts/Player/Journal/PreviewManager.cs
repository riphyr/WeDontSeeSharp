using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PreviewManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Camera previewCamera;
    [SerializeField] private RawImage previewImage;
    [SerializeField] private EntranceAnimator slidePreviewAnimator;
	[SerializeField] private PreviewFaceDetector faceDetector;

    [Header("Options")]
    [SerializeField] private float maxPreviewSize = 1.5f;

    private GameObject currentPreview;
    private bool hasOpenedPreview = false;

    public void ShowPreview(GameObject prefab)
    {
        if (!hasOpenedPreview)
        {
            slidePreviewAnimator.SlideIn();
            hasOpenedPreview = true;
        }

        if (currentPreview != null)
            Destroy(currentPreview);

        GameObject instance = Instantiate(prefab, spawnPoint);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

		Transform inspectFace = FindDeepChild(instance.transform, "InspectFace");

        Renderer rend = instance.GetComponentInChildren<Renderer>();
        if (rend == null) return;

        Vector3 worldCenter = rend.bounds.center;

        GameObject pivot = new GameObject("PreviewPivot");
        pivot.transform.SetParent(spawnPoint, false);
        pivot.transform.position = worldCenter;

        instance.transform.SetParent(pivot.transform, true);
        instance.transform.localPosition = instance.transform.position - worldCenter;
        instance.transform.localRotation = Quaternion.identity;

        float largestSize = rend.bounds.size.magnitude * 2.5f;
        float scaleFactor = maxPreviewSize / largestSize;
        pivot.transform.localScale = Vector3.one * scaleFactor;

        pivot.transform.localScale = Vector3.zero;
        StartCoroutine(AnimateAppear(pivot, Vector3.one * scaleFactor));

        currentPreview = pivot;

        previewCamera.enabled = true;
        pivot.AddComponent<OrbitRotation>();

		if (faceDetector != null)
		{
    		faceDetector.SetFace(inspectFace);
        	faceDetector.ResetPanel();
		}
    }

    IEnumerator AnimateAppear(GameObject obj, Vector3 targetScale)
    {
        float t = 0f;
        float duration = 0.25f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float factor = Mathf.SmoothStep(0, 1, t / duration);
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, factor);
            yield return null;
        }
        obj.transform.localScale = targetScale;
    }

    public void ClearPreview()
    {
        hasOpenedPreview = false;
        if (currentPreview != null)
            Destroy(currentPreview);
    }

	private Transform FindDeepChild(Transform parent, string name)
	{
    	foreach (Transform child in parent)
    	{
        	if (child.name == name) return child;

        	Transform result = FindDeepChild(child, name);
        	if (result != null) return result;
    	}
    	return null;
	}
}
