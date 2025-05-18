using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

namespace InteractionScripts
{
    public class SafeDial : MonoBehaviourPun
    {
        public Transform dialTransform;
        public SafeValve safeValve;
        public AudioClip clickSound;
        public int[] correctCombination;

        private List<int> enteredCombination = new List<int>();
        private int currentNumber = 0;
        private bool isTurningRight = true;
        private AudioSource audioSource;
        public BoxCollider collider;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        
        public void RotateDial(int direction)
        {
            if (!photonView.IsMine)
                photonView.RequestOwnership();

            photonView.RPC("RPC_RotateDial", RpcTarget.AllBuffered, direction);
        }

        [PunRPC]
        private void RPC_RotateDial(int direction)
        {
            if (enteredCombination.Count < correctCombination.Length && isTurningRight != (direction > 0))
            {
                enteredCombination.Add(currentNumber);
            }

            isTurningRight = direction > 0;
            currentNumber = (currentNumber + direction + 100) % 100;

            dialTransform.localEulerAngles = new Vector3(0, 0, currentNumber * 3.6f);

            if (clickSound != null)
                audioSource.PlayOneShot(clickSound);
        }


        public void RegisterCurrentNumber()
        {
            if (enteredCombination.Count < correctCombination.Length)
            {
                enteredCombination.Add(currentNumber);
            }

            if (enteredCombination.Count == correctCombination.Length && CheckCode())
            {
                photonView.RPC("UnlockSafe", RpcTarget.AllBuffered);
            }
        }

        public void ResetCombination()
        {
            photonView.RPC("RPC_ResetCombination", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_ResetCombination()
        {
            enteredCombination.Clear();
            currentNumber = 0;
            isTurningRight = true;

            dialTransform.localEulerAngles = Vector3.zero;
            photonView.RPC("SyncRotation", RpcTarget.Others, dialTransform.localEulerAngles);
        }

        private bool CheckCode()
        {
            for (int i = 0; i < correctCombination.Length; i++)
            {
                if (enteredCombination[i] != correctCombination[i])
                    return false;
            }
            return true;
        }

        [PunRPC]
        private void UnlockSafe()
        {
            collider.enabled = false;

            PhotonView valveView = safeValve.GetComponent<PhotonView>();
            valveView.RPC("RPC_SetUnlocked", RpcTarget.AllBuffered, true);
        }

    }
}
