using UnityEngine;

public class SceneEndTriggerZone : MonoBehaviour
{
    [Header("Référence vers le gestionnaire de fin de scène")]
    [SerializeField] private SceneEndTrigger triggerReceiver;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerReceiver.Trigger();
        }
    }
}