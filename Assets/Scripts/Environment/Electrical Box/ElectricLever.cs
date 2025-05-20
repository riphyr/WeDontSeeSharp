using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace InteractionScripts
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(AudioSource))]
    public class ElectricLever : MonoBehaviourPun
    {
        public Transform voltArrow;
        public float activationAngle = -90f;
        public float deactivationAngle = 30f;
        public float gaugeMinAngle = -45f;
        public float gaugeMaxAngle = 45f;
        private AudioSource audioSource;
        public AudioClip activationSound;
        public AudioClip errorSound;
        public AudioClip runningLoopClip;
        private AudioSource runningLoop;
        public ElectricButton[] buttons;
        public bool[] correctCombination;

        private bool isActive = false;
        public bool IsActive => isActive;
        private bool isMoving = false;
        private PhotonView view;

        private void Start()
        {
            view = GetComponent<PhotonView>();
            audioSource = GetComponent<AudioSource>();

            runningLoop = gameObject.AddComponent<AudioSource>();
            runningLoop.clip = runningLoopClip;
            runningLoop.loop = true;
            runningLoop.playOnAwake = false;
            runningLoop.volume = 0.1f;
        }

        public void TryActivateLever()
        {
            if (isMoving || isActive)
                return;

            view.RPC("RPC_ActivateLever", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_ActivateLever()
        {
            StartCoroutine(ActivateLever());
        }

        private IEnumerator ActivateLever()
        {
            isMoving = true;

            Collider leverCollider = GetComponent<Collider>();
            leverCollider.enabled = false;
            foreach (ElectricButton btn in buttons)
                btn.GetComponent<Collider>().enabled = false;

            // Vérification de la combinaison correcte
            int correctCount = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].IsOn() == correctCombination[i])
                    correctCount++;
            }

            float startNeedleAngle = voltArrow.localRotation.eulerAngles.z;
            float targetNeedleAngle = Mathf.Lerp(gaugeMinAngle, gaugeMaxAngle, (float)correctCount / buttons.Length);
            bool success = correctCount == buttons.Length;

            // Étape 1 : Lever le levier et l'aiguille en même temps
            Quaternion startRotation = transform.localRotation;
            Quaternion targetRotation = Quaternion.Euler(activationAngle, 0, 0);
            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime * 3f;
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

                float newNeedleAngle = Mathf.Lerp(startNeedleAngle, targetNeedleAngle, t);
                voltArrow.localRotation = Quaternion.Euler(-90, 0, newNeedleAngle);

                yield return null;
            }

            if (success)
            {
                isActive = true;
                audioSource.PlayOneShot(activationSound);

                if (!runningLoop.isPlaying)
                {
                    runningLoop.Play();
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                audioSource.PlayOneShot(errorSound);
                yield return new WaitForSeconds(1f);

                startRotation = transform.localRotation;
                targetRotation = Quaternion.Euler(deactivationAngle, 0, 0);
                float needleStart = voltArrow.localRotation.eulerAngles.z;
                float needleEnd = gaugeMinAngle;
                t = 0;

                while (t < 1)
                {
                    t += Time.deltaTime * 3f;
                    transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

                    float newNeedleAngle = Mathf.Lerp(needleStart, needleEnd, t);
                    voltArrow.localRotation = Quaternion.Euler(-90, 0, newNeedleAngle);

                    yield return null;
                }

                runningLoop.Stop();
            }

            leverCollider.enabled = true;
            foreach (ElectricButton btn in buttons)
                btn.GetComponent<Collider>().enabled = true;

            isMoving = false;
        }
        
        [PunRPC]
        private void RPC_ResetLever()
        {
            StartCoroutine(ResetLeverRoutine());
        }

        public void ResetLeverManually()
        {
            if (isMoving)
                return;

            view.RPC("RPC_ResetLever", RpcTarget.All);
        }

        private IEnumerator ResetLeverRoutine()
        {
            isMoving = true;

            Collider leverCollider = GetComponent<Collider>();
            leverCollider.enabled = false;

            yield return new WaitForSeconds(0.5f);
            audioSource.PlayOneShot(errorSound);
            yield return new WaitForSeconds(1f);
            
            float startNeedleAngle = voltArrow.localRotation.eulerAngles.z;
            float startLeverAngle = transform.localRotation.eulerAngles.x;

            Quaternion startRotation = transform.localRotation;
            Quaternion targetRotation = Quaternion.Euler(deactivationAngle, 0, 0);
            float needleStart = startNeedleAngle;
            float needleEnd = gaugeMinAngle;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 3f;
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

                float newNeedleAngle = Mathf.Lerp(needleStart, needleEnd, t);
                voltArrow.localRotation = Quaternion.Euler(-90, 0, newNeedleAngle);

                yield return null;
            }

            isActive = false;
            runningLoop.Stop();

            leverCollider.enabled = true;
            isMoving = false;
            
            // Appel à tous les switches pour les forcer à OFF
            var allSwitches = FindObjectsOfType<InteractionScripts.Switch>();
            foreach (var sw in allSwitches)
            {
                sw.photonView.RPC("ForceSwitchOff", RpcTarget.All);
            }
        }
    }
}
