using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MainMenu{
    public class KeyBindingManager : MonoBehaviour {
        
        [Header("PANEL")]
        public GameObject keyConfirmationPanel;
        
        [Header("KEY NAMES")]
        public GameObject forwardtext;
        public GameObject backwardtext;
        public GameObject lefttext;
        public GameObject righttext;
        public GameObject jumptext;
        public GameObject sprinttext;
        public GameObject nextinventorytext;
        public GameObject previousinventorytext;
        public GameObject maptext;
        public GameObject usetext;
        public GameObject primaryInteractionText;
        public GameObject secondaryInteractionText;
        public GameObject reloadtext;
        public GameObject droptext;
        public GameObject pausetext;
        public GameObject inventorytext;

        private Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();
        private Dictionary<string, TMP_Text> keyBindingTexts = new Dictionary<string, TMP_Text>();

        private string currentKeyBinding; 

        void Start()
        {
            keyBindingTexts["Forward"] = forwardtext.GetComponent<TMP_Text>();
            keyBindingTexts["Backward"] = backwardtext.GetComponent<TMP_Text>();
            keyBindingTexts["Left"] = lefttext.GetComponent<TMP_Text>();
            keyBindingTexts["Right"] = righttext.GetComponent<TMP_Text>();
            keyBindingTexts["Jump"] = jumptext.GetComponent<TMP_Text>();
            keyBindingTexts["Sprint"] = sprinttext.GetComponent<TMP_Text>();
            keyBindingTexts["Next"] = nextinventorytext.GetComponent<TMP_Text>();
            keyBindingTexts["Previous"] = previousinventorytext.GetComponent<TMP_Text>();
            keyBindingTexts["Map"] = maptext.GetComponent<TMP_Text>();
            keyBindingTexts["Use"] = usetext.GetComponent<TMP_Text>();
            keyBindingTexts["PrimaryInteraction"] = primaryInteractionText.GetComponent<TMP_Text>();
            keyBindingTexts["SecondaryInteraction"] = secondaryInteractionText.GetComponent<TMP_Text>();
            keyBindingTexts["Reload"] = reloadtext.GetComponent<TMP_Text>();
            keyBindingTexts["Drop"] = droptext.GetComponent<TMP_Text>();
            keyBindingTexts["Pause"] = pausetext.GetComponent<TMP_Text>();
            keyBindingTexts["Inventory"] = inventorytext.GetComponent<TMP_Text>();

            LoadKeyBindings();
        }
        
        void Update()
        {
            if (keyConfirmationPanel.activeSelf && currentKeyBinding != null)
            {
                foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        keyBindings[currentKeyBinding] = keyCode;
                        SaveKeyBindings();
                        UpdateKeyBindingText(currentKeyBinding, keyCode);
                        keyConfirmationPanel.SetActive(false);
                        currentKeyBinding = null;
                        break;
                    }
                }
            }
        }

        public void OnKeyBindingButtonClicked(string keyBindingName)
        {
            currentKeyBinding = keyBindingName;
            keyConfirmationPanel.SetActive(true);
        }
        
        public void CancelButton()
        {
            keyConfirmationPanel.SetActive(false);
            currentKeyBinding = null;
        }

        private void SaveKeyBindings()
        {
            foreach (var binding in keyBindings)
            {
                PlayerPrefs.SetString(binding.Key, binding.Value.ToString());
            }

            PlayerPrefs.Save();
        }

        private void LoadKeyBindings()
        {
            Dictionary<string, KeyCode> defaultBindings = new Dictionary<string, KeyCode>
            {
                { "Forward", KeyCode.Z },
                { "Backward", KeyCode.S },
                { "Left", KeyCode.Q },
                { "Right", KeyCode.D },
                { "Jump", KeyCode.Space },
                { "Sprint", KeyCode.LeftShift },
                { "Next", KeyCode.RightArrow },
                { "Previous", KeyCode.LeftArrow },
                { "Map", KeyCode.M },
                { "Use", KeyCode.A },
                { "PrimaryInteraction", KeyCode.E },
                { "SecondaryInteraction", KeyCode.F },
                { "Reload", KeyCode.R },
                { "Drop", KeyCode.T },
                { "Pause", KeyCode.Escape },
                { "Inventory", KeyCode.I }
            };

            foreach (var action in defaultBindings.Keys)
            {
                string savedKey = PlayerPrefs.GetString(action, "None");
                if (System.Enum.TryParse(savedKey, out KeyCode keyCode) && keyCode != KeyCode.None)
                {
                    keyBindings[action] = keyCode;
                }
                else
                {
                    keyBindings[action] = defaultBindings[action];
                    PlayerPrefs.SetString(action, defaultBindings[action].ToString());
                }
                UpdateKeyBindingText(action, keyBindings[action]);
            }

            PlayerPrefs.Save();
        }


        private void UpdateKeyBindingText(string action, KeyCode keyCode)
        {
            if (keyBindingTexts.ContainsKey(action))
            {
                keyBindingTexts[action].text = keyCode.ToString();
            }
        }
    }
}
