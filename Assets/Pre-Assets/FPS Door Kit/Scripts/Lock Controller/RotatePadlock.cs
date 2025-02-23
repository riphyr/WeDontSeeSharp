
/* ~ FPS Door Kit V1.0 ~ */

using System.Collections;
using UnityEngine;

namespace EnivStudios
{
    public class RotatePadlock : MonoBehaviour
    {
        public int gearNumber;
        private bool coroutineAllowed = true;

        LockController lockController;
        int numberShown = 0;
        private void Start()
        {
            lockController = GetComponentInParent<LockController>();
        }
        private void Update()
        {
            if (lockController.padLockGear) { gameObject.GetComponent<Collider>().enabled = true; }
            else  { gameObject.GetComponent<Collider>().enabled = false; }
        }
        private void OnMouseDown()
        {
            if (coroutineAllowed && !lockController.padLockStop)
            {
                lockController.audioSource.clip = lockController.rotateSound;
                lockController.audioSource.Play();
                StartCoroutine(RotateWheel());
            }
        }
        private IEnumerator RotateWheel()
        {
            coroutineAllowed = false;

            for (int i = 0; i <= 11; i++)
            {
                transform.Rotate(0, 3, 0);
                yield return new WaitForSeconds(0.01f);
            }

            coroutineAllowed = true;

            numberShown += 1;

            if (numberShown > 9)
            {
                numberShown = 0;
            }
           lockController.PadlockNo(gearNumber,numberShown);
        }
    }
}