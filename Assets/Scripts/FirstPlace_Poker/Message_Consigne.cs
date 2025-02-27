namespace GameTest
{
    using UnityEngine;
    using TMPro;
    using System.Collections;
    using Photon.Pun; 

    public class Message_Consigne : MonoBehaviourPunCallbacks
    {
        public GameObject messagePanel;
        public TextMeshProUGUI messageText;
        public float Duration = 3f;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return; 

            PhotonView playerPhotonView = other.GetComponent<PhotonView>();

            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                messagePanel.SetActive(true);
                messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1); // Remet l'alpha à 1
                StopAllCoroutines();
                StartCoroutine(FadeOutText(15f)); 
            }
        }

        private IEnumerator FadeOutText(float delay)
        {
            yield return new WaitForSeconds(delay); 

            float disparition = 0f;
            Color originalColor = messageText.color;

            while (disparition < Duration)
            {
                disparition += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, disparition / Duration);
                messageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            messagePanel.SetActive(false); 
        }
    }

}