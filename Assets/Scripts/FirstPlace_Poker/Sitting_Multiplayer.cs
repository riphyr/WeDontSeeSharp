using UnityEngine;
using TMPro;
using System.Collections;

namespace GameTest
{
    public class Sitting_SinglePlayer : MonoBehaviour
    {
        [Header("Player States")]
        public GameObject playerStanding;
        public GameObject playerSitting;
        public string pokerIntroMessage = "Welcome to the table, where fortunes are made and lives are shattered. In this game, every bluff could be your last, and every hand could be the end of everything you hold dear.";  
        public string gameNotStartingMessage = "The game doesn't begin just yet. Wait for your turn, or risk everything on a single bad decision.";  
        private bool messageDisplayed = false;
        public float typingSpeed = 0.1f;  

        [Header("UI Elements")] 
        public GameObject intText;
        public TextMeshProUGUI messageUI;  

        private bool interactable = false;

        [Header("Trap Settings")]
        public Transform trapTriggerArea;  
        public float fallDelay = 0f;  

        public Transform landingPoint;  

        private bool isPlayerInTrapZone = false;
        private GameObject player;
        private Rigidbody playerRigidbody;


        private void Start()
        {
            messageUI.text = "";  
            messageUI.gameObject.SetActive(false);  
        }

        private void OnTriggerEnter(Collider other)
        {
            intText.SetActive(true);
            interactable = true;
        }

        private void OnTriggerExit(Collider other)
        {
            intText.SetActive(false);
            interactable = false;
        }

        private void Update()
        {
            if (interactable && Input.GetKeyDown(KeyCode.E) && !playerSitting.activeSelf)
            {
                intText.SetActive(false);
                Sit();
            }
        }

        private void Sit()
        {
            playerStanding.SetActive(false);
            playerSitting.SetActive(true);
            if (!messageDisplayed)
            {
                StartCoroutine(DisplayPokerIntro());
            }
        
            
        
        }

        private IEnumerator DisplayPokerIntro()
        {
            messageUI.gameObject.SetActive(true);  
            messageUI.text = "";  
            for (int i = 0; i < pokerIntroMessage.Length; i++)
            {
                messageUI.text += pokerIntroMessage[i];
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(6f);  

            messageUI.text = "";  
            for (int i = 0; i < gameNotStartingMessage.Length; i++)
            {
                messageUI.text += gameNotStartingMessage[i];  
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(6f);  
            messageUI.gameObject.SetActive(false);
            messageDisplayed = true;  
            player = playerSitting;  
            playerRigidbody = player.GetComponent<Rigidbody>();
            isPlayerInTrapZone = true;  
            StartCoroutine(ActivateTrap());
        }

        private IEnumerator ActivateTrap()
        {
            yield return new WaitForSeconds(fallDelay);
            if (isPlayerInTrapZone && player != null)
            {
                FallIntoTrap();
            }
        }

        private void FallIntoTrap()
        {
            if (landingPoint != null) 
            {
                player.transform.position = landingPoint.position;  
            }
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = false;  
            }
        }
    }
}





/*using UnityEngine;
using TMPro; 
using System.Collections;

namespace GameTest
{
    public class Sitting_SinglePlayer : MonoBehaviour
    {
        [Header("Player States")] 
        public GameObject playerStanding;
        public GameObject playerSitting;
        public string pokerIntroMessage = "Welcome to the table, where fortunes are made and lives are shattered. In this game, every bluff could be your last, and every hand could be the end of everything you hold dear.";  
        public string gameNotStartingMessage = "The game doesn't begin just yet. Wait for your turn, or risk everything on a single bad decision.";  
        private bool messageDisplayed = false;
        public float typingSpeed = 0.1f;  

        [Header("UI Elements")] 
        public GameObject intText;
        public TextMeshProUGUI messageUI;  

        private bool interactable = false;

        [Header("Trap Settings")]
        public Transform trapTriggerArea;  
        public float fallDelay = 5f;  
        public Vector3 fallPositionOffset = new Vector3(0, -10, 0);  

        private bool isPlayerInTrapZone = false;
        private GameObject player;


        private void Start()
        {
            messageUI.text = "";  
            messageUI.gameObject.SetActive(false);  
        }

        private void OnTriggerEnter(Collider other)
        {
            intText.SetActive(true);
            interactable = true;
        }

        private void OnTriggerExit(Collider other)
        {
            intText.SetActive(false);
            interactable = false;
        }

        private void Update()
        {
            if (interactable && Input.GetKeyDown(KeyCode.E) && !playerSitting.activeSelf)
            {
                intText.SetActive(false);
                Sit();
            }
        }

        private void Sit()
        {   
            intText.SetActive(false);
            playerStanding.SetActive(false);
            playerSitting.SetActive(true);
            if (!messageDisplayed)
            {
                StartCoroutine(DisplayPokerIntro());
            }
            player = other.gameObject;  
            isPlayerInTrapZone = true;  
            StartCoroutine(ActivateTrap());
        }

        private IEnumerator DisplayPokerIntro()
        {
            messageUI.gameObject.SetActive(true);  
            messageUI.text = "";  
            for (int i = 0; i < pokerIntroMessage.Length; i++)
            {
                messageUI.text += pokerIntroMessage[i];
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(6f);  

            messageUI.text = "";  
            for (int i = 0; i < gameNotStartingMessage.Length; i++)
            {
                messageUI.text += gameNotStartingMessage[i];  
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(6f);  
            messageUI.gameObject.SetActive(false);
            messageDisplayed = true;  
        }

         private IEnumerator ActivateTrap()
    {
        yield return new WaitForSeconds(fallDelay);
        if (isPlayerInTrapZone && player != null)
        {
            FallIntoTrap();
        }
    }

    private void FallIntoTrap()
    {
        player.transform.position += fallPositionOffset;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;  
        }
    }
    }
}
*/
