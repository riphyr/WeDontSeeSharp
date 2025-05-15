using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace InteractionScripts
{
    public class ElectricLeverAutoReset : MonoBehaviourPun
    {
        [SerializeField] private ElectricLever electricLever;
        [SerializeField] private float resetTimeMin = 300f; // 5 minutes
        [SerializeField] private float resetTimeMax = 600f; // 10 minutes

        private Coroutine resetCoroutine;

        private void Start()
        {
            if (electricLever == null)
                electricLever = GetComponent<ElectricLever>();
        }

        private void Update()
        {
            if (electricLever == null)
                return;

            if (electricLever.IsActive && resetCoroutine == null)
            {
                resetCoroutine = StartCoroutine(DelayedReset());
            }

            if (!electricLever.IsActive && resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
        }

        private IEnumerator DelayedReset()
        {
            float delay = Random.Range(resetTimeMin, resetTimeMax);
            Debug.Log($"⏳ Démarrage du compte à rebours pour reset dans {delay} secondes.");

            yield return new WaitForSeconds(delay);

            if (electricLever.IsActive)
            {
                Debug.Log("🔔 Temps écoulé, reset automatique du levier.");
                electricLever.ResetLeverManually();
            }

            resetCoroutine = null;
        }
    }
}