using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


namespace PauseMenu{
	public class PauseMenuManager : MonoBehaviour {
		
		[Header("MENUS")]
        public GameObject pauseObject;			// Canva global
        public GameObject mainCanva;			// Panel MAIN
        public GameObject settingsCanva;		// Panel PLAY
        public GameObject exitMenu;				// Panel EXIT

        [Header("PANELS")]
        public GameObject PanelGame;			// Panel GAME
        public GameObject PanelControls;		// Panel CONTROLS
        public GameObject PanelVideo;			// Panel VIDEO
        public GameObject PanelKeyBindings;		// Panel KEYBINDINGS
        public GameObject PanelMovement;		// Panel MOVEMENT
        public GameObject PanelInteractions;	// Panel INTERACTIONS
        public GameObject PanelGeneral;			// Panel GENERAL
        
        [Header("SETTINGS SCREEN")]
        public GameObject lineGame;
        public GameObject lineControls;
        public GameObject lineVideo;
        public GameObject lineKeyBindings;
        public GameObject lineMovement;
        public GameObject lineInteractions;
        public GameObject lineGeneral;

		[Header("SFX")]
        public AudioSource hoverSound;
        
		[Header("VIDEO SETTINGS")]
		public GameObject fullscreentext;
		public GameObject shadowofftextLINE;
		public GameObject shadowlowtextLINE;
		public GameObject shadowhightextLINE;
		public GameObject vsynctext;
		public GameObject texturelowtextLINE;
		public GameObject texturemedtextLINE;
		public GameObject texturehightextLINE;
		public TMP_Dropdown ResolutionDropdown;

		[Header("GAME SETTINGS")]
		public GameObject tooltipstext;
		public GameObject musicSlider;
		
		[Header("CONTROLS SETTINGS")]
		public GameObject sensitivityXSlider;
		public GameObject sensitivityYSlider;
		
		[Header("PANEL KEYBINDINGS")]
		public GameObject keyConfirmationPanel;
		public GameObject keyConfirmationBackground;
        
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
		public GameObject journaltext;
		
		//Sliders
		private float sliderValue = 0.0f;
		private float sliderValueXSensitivity = 0.5f;
		private float sliderValueYSensitivity = 0.5f;
		
		private Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();
		private Dictionary<string, TMP_Text> keyBindingTexts = new Dictionary<string, TMP_Text>();

		private string currentKeyBinding; 
		private bool isPaused = false;
		private static bool blockNextEscape = false;

		void Start(){
			pauseObject.SetActive(false);
			isPaused = false;
			
			settingsCanva.SetActive(false);
			exitMenu.SetActive(false);
			mainCanva.SetActive(false);
			
			// Vérification des sliders
			musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume");
			GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
			sensitivityXSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("XSensitivity");
			sensitivityYSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("YSensitivity");

			// Vérification du fullscreen
			fullscreentext.GetComponent<TMP_Text>().text = Screen.fullScreen ? "off" : "on";
			
			// Vérification des tooltips
			tooltipstext.GetComponent<TMP_Text>().text = PlayerPrefs.GetInt("ToolTips") == 0 ? "off" : "on";

			// Vérification des ombres
			UpdateShadowSettings();

			// Vérification de la Vsync
			vsynctext.GetComponent<TMP_Text>().text = QualitySettings.vSyncCount == 0 ? "off" : "on";

			// Vérification des qualités textures
			UpdateTextureSettings();
			
			// Vérification de la résolution
			int savedResolution = PlayerPrefs.GetInt("Resolution", 0);
			ResolutionDropdown.value = savedResolution;
			ApplyResolution(savedResolution);
			
			//Application des touches
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
			keyBindingTexts["Journal"] = journaltext.GetComponent<TMP_Text>();

			LoadKeyBindings();
		}
		
		public void Update (){
			sliderValue = musicSlider.GetComponent<Slider>().value;
			sliderValueXSensitivity = sensitivityXSlider.GetComponent<Slider>().value;
			sliderValueYSensitivity = sensitivityYSlider.GetComponent<Slider>().value;
			
			ApplyMouseSensitivity();
			
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
						keyConfirmationBackground.SetActive(false);
						currentKeyBinding = null;
						break;
					}
				}
				if (currentKeyBinding == null)
					return;
			}

			if (blockNextEscape)
			{
				blockNextEscape = false;
				return;
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (isPaused)
				{
					if (settingsCanva.activeSelf && !keyConfirmationPanel.activeSelf)
						ReturnButton();
					else if (mainCanva.activeSelf)
						ResumeButton();
				}
				else
				{
					PauseGame();
				}
			}
		}

		public static void BlockNextEscapePress()
		{
			blockNextEscape = true;
		}

		public void UpdateVolume()
		{
            GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
        }
		
		public void ResumeButton()
		{
			isPaused = false;
			pauseObject.SetActive(false);

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		
		private void PauseGame()
		{
			isPaused = true;
			pauseObject.SetActive(true);

			mainCanva.SetActive(true);
			settingsCanva.SetActive(false);
			exitMenu.SetActive(false);

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public void LoadScene(string scene)
    	{
        	if (scene != "")
        	{
            	if (PhotonNetwork.IsConnected)
            	{
                	PhotonNetwork.Disconnect();
                	StartCoroutine(WaitForDisconnectAndLoadScene(scene));
            	}
            	else
            	{
                	SceneManager.LoadSceneAsync(scene);
            	}
        	}
    	}

    	private IEnumerator WaitForDisconnectAndLoadScene(string scene)
    	{
        	while (PhotonNetwork.IsConnected)
        	{
            	yield return null;
        	}
        	SceneManager.LoadSceneAsync(scene);
    	}

		public void SettingButton(){
			settingsCanva.SetActive(true);
			mainCanva.SetActive(false);
		}
		public void ReturnButton(){
			settingsCanva.SetActive(false);
			exitMenu.SetActive(false);
			mainCanva.SetActive(true);
			pauseObject.SetActive(true);
		}
		
		public void AreYouSure(){
			exitMenu.SetActive(true);
		}

		public void QuitGame(){
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}
		void DisablePanels(){
			PanelControls.SetActive(false);
			PanelVideo.SetActive(false);
			PanelGame.SetActive(false);
			PanelKeyBindings.SetActive(false);

			lineGame.SetActive(false);
			lineControls.SetActive(false);
			lineVideo.SetActive(false);
			lineKeyBindings.SetActive(false);

			PanelMovement.SetActive(false);
			lineMovement.SetActive(false);
			PanelInteractions.SetActive(false);
			lineInteractions.SetActive(false);
			PanelGeneral.SetActive(false);
			lineGeneral.SetActive(false);
		}

		public void GamePanel(){
			DisablePanels();
			PanelGame.SetActive(true);
			lineGame.SetActive(true);
		}

		public void VideoPanel(){
			DisablePanels();
			PanelVideo.SetActive(true);
			lineVideo.SetActive(true);
		}

		public void ControlsPanel(){
			DisablePanels();
			PanelControls.SetActive(true);
			lineControls.SetActive(true);
		}

		public void KeyBindingsPanel(){
			DisablePanels();
			MovementPanel();
			PanelKeyBindings.SetActive(true);
			lineKeyBindings.SetActive(true);
		}

		public void MovementPanel(){
			DisablePanels();
			PanelKeyBindings.SetActive(true);
			PanelMovement.SetActive(true);
			lineMovement.SetActive(true);
		}

		public void InteractionsPanel(){
			DisablePanels();
			PanelKeyBindings.SetActive(true);
			PanelInteractions.SetActive(true);
			lineInteractions.SetActive(true);
		}

		public void GeneralPanel(){
			DisablePanels();
			PanelKeyBindings.SetActive(true);
			PanelGeneral.SetActive(true);
			lineGeneral.SetActive(true);
		}

		public void PlayHover(){
			hoverSound.Play();
		}
		
		public void SetResolution()
		{
			int selectedResolution = ResolutionDropdown.value;
			ApplyResolution(selectedResolution);
			PlayerPrefs.SetInt("Resolution", selectedResolution);
		}

		private void ApplyResolution(int resolutionIndex)
		{
			switch (resolutionIndex)
			{
				case 0:
					Screen.SetResolution(1920, 1080, Screen.fullScreen);
					break;
				case 1:
					Screen.SetResolution(1280, 720, Screen.fullScreen);
					break;
				case 2:
					Screen.SetResolution(854, 480, Screen.fullScreen);
					break;
				case 3:
					Screen.SetResolution(640, 360, Screen.fullScreen);
					break;
			}
		}
			
		public void ApplyMouseSensitivity() {
			float mouseSensitivityX = sliderValueXSensitivity * 2.0f;
			float mouseSensitivityY = sliderValueYSensitivity * 2.0f;
			
			PlayerPrefs.SetFloat("sensitivityX", mouseSensitivityX);
			PlayerPrefs.SetFloat("sensitivityY", mouseSensitivityY);
		}

		public void FullScreen (){
			Screen.fullScreen = !Screen.fullScreen;
			fullscreentext.GetComponent<TMP_Text>().text = fullscreentext.GetComponent<TMP_Text>().text == "on" ? "off" : "on";
		}

		public void MusicSlider (){
			PlayerPrefs.SetFloat("MusicVolume", musicSlider.GetComponent<Slider>().value);
		}

		public void SensitivityXSlider (){
			PlayerPrefs.SetFloat("XSensitivity", sliderValueXSensitivity);
		}

		public void SensitivityYSlider (){
			PlayerPrefs.SetFloat("YSensitivity", sliderValueYSensitivity);
		}

		// show tool tips like: 'How to Play' control pop ups
		public void ToolTips (){
			int currentState = PlayerPrefs.GetInt("ToolTips");
			PlayerPrefs.SetInt("ToolTips", 1 - currentState);
			tooltipstext.GetComponent<TMP_Text>().text = PlayerPrefs.GetInt("ToolTips") == 0 ? "off" : "on";
		}
		
		// Update Shadow Quality
		public void UpdateShadowSettings() {
			int shadowSetting = PlayerPrefs.GetInt("Shadows");
			switch (shadowSetting) {
				case 0:
					QualitySettings.shadowCascades = 0;
					QualitySettings.shadowDistance = 0;
					shadowofftextLINE.gameObject.SetActive(true);
					shadowlowtextLINE.gameObject.SetActive(false);
					shadowhightextLINE.gameObject.SetActive(false);
					break;
				case 1:
					QualitySettings.shadowCascades = 2;
					QualitySettings.shadowDistance = 75;
					shadowofftextLINE.gameObject.SetActive(false);
					shadowlowtextLINE.gameObject.SetActive(true);
					shadowhightextLINE.gameObject.SetActive(false);
					break;
				case 2:
					QualitySettings.shadowCascades = 4;
					QualitySettings.shadowDistance = 500;
					shadowofftextLINE.gameObject.SetActive(false);
					shadowlowtextLINE.gameObject.SetActive(false);
					shadowhightextLINE.gameObject.SetActive(true);
					break;
			}
		}
		
		// Update Texture Quality
		public void UpdateTextureSettings() {
			int textureSetting = PlayerPrefs.GetInt("Textures");
			switch (textureSetting) {
				case 0:
					QualitySettings.globalTextureMipmapLimit = 2;
					texturelowtextLINE.gameObject.SetActive(true);
					texturemedtextLINE.gameObject.SetActive(false);
					texturehightextLINE.gameObject.SetActive(false);
					break;
				case 1:
					QualitySettings.globalTextureMipmapLimit = 1;
					texturelowtextLINE.gameObject.SetActive(false);
					texturemedtextLINE.gameObject.SetActive(true);
					texturehightextLINE.gameObject.SetActive(false);
					break;
				case 2:
					QualitySettings.globalTextureMipmapLimit = 0;
					texturelowtextLINE.gameObject.SetActive(false);
					texturemedtextLINE.gameObject.SetActive(false);
					texturehightextLINE.gameObject.SetActive(true);
					break;
			}
		}
		
		public void ShadowsOff (){
			PlayerPrefs.SetInt("Shadows",0);
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0;
			shadowofftextLINE.gameObject.SetActive(true);
			shadowlowtextLINE.gameObject.SetActive(false);
			shadowhightextLINE.gameObject.SetActive(false);
		}

		public void ShadowsLow (){
			PlayerPrefs.SetInt("Shadows",1);
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 75;
			shadowofftextLINE.gameObject.SetActive(false);
			shadowlowtextLINE.gameObject.SetActive(true);
			shadowhightextLINE.gameObject.SetActive(false);
		}

		public void ShadowsHigh (){
			PlayerPrefs.SetInt("Shadows",2);
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 500;
			shadowofftextLINE.gameObject.SetActive(false);
			shadowlowtextLINE.gameObject.SetActive(false);
			shadowhightextLINE.gameObject.SetActive(true);
		}

		public void vsync (){
			if(QualitySettings.vSyncCount == 0){
				QualitySettings.vSyncCount = 1;
				vsynctext.GetComponent<TMP_Text>().text = "on";
			}
			else if(QualitySettings.vSyncCount == 1){
				QualitySettings.vSyncCount = 0;
				vsynctext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void TexturesLow (){
			PlayerPrefs.SetInt("Textures",0);
			QualitySettings.globalTextureMipmapLimit = 2;
			texturelowtextLINE.gameObject.SetActive(true);
			texturemedtextLINE.gameObject.SetActive(false);
			texturehightextLINE.gameObject.SetActive(false);
		}

		public void TexturesMed (){
			PlayerPrefs.SetInt("Textures",1);
			QualitySettings.globalTextureMipmapLimit = 1;
			texturelowtextLINE.gameObject.SetActive(false);
			texturemedtextLINE.gameObject.SetActive(true);
			texturehightextLINE.gameObject.SetActive(false);
		}

		public void TexturesHigh (){
			PlayerPrefs.SetInt("Textures",2);
			QualitySettings.globalTextureMipmapLimit = 0;
			texturelowtextLINE.gameObject.SetActive(false);
			texturemedtextLINE.gameObject.SetActive(false);
			texturehightextLINE.gameObject.SetActive(true);
		}
		
		public void OnKeyBindingButtonClicked(string keyBindingName)
        {
            currentKeyBinding = keyBindingName;
            keyConfirmationPanel.SetActive(true);
            keyConfirmationBackground.SetActive(true);
        }
        
        public void CancelButton()
        {
            keyConfirmationPanel.SetActive(false);
            keyConfirmationBackground.SetActive(false);
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
	            { "Inventory", KeyCode.I },
	            { "Journal", KeyCode.J }
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

        public Dictionary<string, KeyCode> GetKeyBindings()
        {
	        return keyBindings;
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