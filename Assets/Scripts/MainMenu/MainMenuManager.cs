using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace MainMenu{
	public class MainMenuManager : MonoBehaviour {
		private Animator CameraObject;

        [Header("MENUS")]
        public GameObject mainMenu;				// Canva global
        public GameObject firstMenu;			// Panel MAIN
        public GameObject playMenu;				// Panel PLAY
        public GameObject exitMenu;				// Panel EXIT

        [Header("PANELS")]
        public GameObject mainCanvas;			// Canva MAIN
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
        public AudioSource sliderSound;
        public AudioSource swooshSound;

		void Start(){
			CameraObject = transform.GetComponent<Animator>();

			playMenu.SetActive(false);
			exitMenu.SetActive(false);
			firstMenu.SetActive(true);
			mainMenu.SetActive(true);
		}

		public void Play()
		{
			exitMenu.SetActive(false);
			playMenu.SetActive(true);
		}

		public void ReturnMenu(){
			playMenu.SetActive(false);
			exitMenu.SetActive(false);
			mainMenu.SetActive(true);
		}

		public void LoadScene(string scene){
			if(scene != "")
			{
				SceneManager.LoadSceneAsync(scene);
			}
		}

		public void  DisablePlay(){
			playMenu.SetActive(false);
		}
		
		public void Position1(){ 
			CameraObject.SetFloat("Animate",0);
		}
		public void Position2(){
			DisablePlay();
			CameraObject.SetFloat("Animate",1);
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

		public void PlaySFXHover(){
			sliderSound.Play();
		}

		public void PlaySwoosh(){
			swooshSound.Play();
		}

		public void AreYouSure(){
			exitMenu.SetActive(true);
			DisablePlay();
		}

		public void QuitGame(){
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}
	}
}