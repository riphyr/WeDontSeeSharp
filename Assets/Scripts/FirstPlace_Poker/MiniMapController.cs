using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] private GameObject miniMapUI;
    [SerializeField] private GameObject miniMapDisplay;
    public void HideMiniMap()
    {
        Debug.Log("Hide MiniMap");
        if (miniMapUI != null)
        {
            Destroy(miniMapDisplay);
            Destroy(miniMapUI);
        }
    }

    public void ShowMiniMap()
    {
        if (miniMapUI != null)
        {
            miniMapDisplay.SetActive(true);
            miniMapUI.SetActive(true);
        }
    }
}

