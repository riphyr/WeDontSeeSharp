using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace MainMenu{
	public class SettingsMenuManager : MonoBehaviour {

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
		public GameObject difficultynormaltext;
		public GameObject difficultynormaltextLINE;
		public GameObject difficultyhardcoretext;
		public GameObject difficultyhardcoretextLINE;

		[Header("CONTROLS SETTINGS")]
		// sliders
		public GameObject musicSlider;
		public GameObject sensitivityXSlider;
		public GameObject sensitivityYSlider;

		private float sliderValue = 0.0f;
		private float sliderValueXSensitivity = 0.5f;
		private float sliderValueYSensitivity = 0.5f;
		

		public void  Start (){
			// Verification de la difficulté
			if(PlayerPrefs.GetInt("NormalDifficulty") == 1){
				difficultynormaltextLINE.gameObject.SetActive(true);
				difficultyhardcoretextLINE.gameObject.SetActive(false);
			}
			else
			{
				difficultyhardcoretextLINE.gameObject.SetActive(true);
				difficultynormaltextLINE.gameObject.SetActive(false);
			}

			// Vérification des sliders
			musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume");
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
		}

		public void Update (){
			sliderValue = musicSlider.GetComponent<Slider>().value;
			sliderValueXSensitivity = sensitivityXSlider.GetComponent<Slider>().value;
			sliderValueYSensitivity = sensitivityYSlider.GetComponent<Slider>().value;
			
			ApplyMouseSensitivity();
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

		public void NormalDifficulty (){
			difficultyhardcoretextLINE.gameObject.SetActive(false);
			difficultynormaltextLINE.gameObject.SetActive(true);
			PlayerPrefs.SetInt("NormalDifficulty",1);
		}

		public void HardcoreDifficulty (){
			difficultyhardcoretextLINE.gameObject.SetActive(true);
			difficultynormaltextLINE.gameObject.SetActive(false);
			PlayerPrefs.SetInt("NormalDifficulty",0);
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
	}
}