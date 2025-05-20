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

            yield return new WaitForSeconds(delay);

            if (electricLever.IsActive)
            {
                electricLever.ResetLeverManually();
            }

            resetCoroutine = null;
        }
    }
}