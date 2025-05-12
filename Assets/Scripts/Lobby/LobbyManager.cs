using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace Lobby{
	public class LobbyManager : MonoBehaviour {

        [Header("MENUS")] 
        public GameObject createMenu; 
        public GameObject joinMenu;
        public GameObject loadMenu;
        
        [Header("WARNING")] 
        public GameObject errorMessage;
        public GameObject errorMessageLoad;
        
        [Header("HIGHLIGHT")] 
        public GameObject lineCreate;
        public GameObject lineJoin;
        public GameObject lineLoad;
        public GameObject evolvingDetails;
        
		[Header("SFX")]
        public AudioSource hoverSound;
        
		[Header("GAME SETTINGS")]
		public GameObject difficultynormaltext;
		public GameObject difficultynormaltextLINE;
		public GameObject difficultyhardcoretext;
		public GameObject difficultyhardcoretextLINE;

		void Start(){
			createMenu.SetActive(false);
			joinMenu.SetActive(false);
			loadMenu.SetActive(false);
			evolvingDetails.SetActive(false);
			errorMessage.SetActive(false);
			errorMessageLoad.SetActive(false);
			
			if(PlayerPrefs.GetInt("NormalDifficultyMulti") == 1){
				difficultynormaltextLINE.gameObject.SetActive(true);
				difficultyhardcoretextLINE.gameObject.SetActive(false);
			}
			else
			{
				difficultyhardcoretextLINE.gameObject.SetActive(true);
				difficultynormaltextLINE.gameObject.SetActive(false);
			}
		}

		public void NormalDifficultyMulti (){
			difficultyhardcoretextLINE.gameObject.SetActive(false);
			difficultynormaltextLINE.gameObject.SetActive(true);
			PlayerPrefs.SetInt("NormalDifficultyMulti",1);
			PlayerPrefs.SetInt("HardCoreDifficultyMulti",0);
		}

		public void HardcoreDifficultyMulti (){
			difficultyhardcoretextLINE.gameObject.SetActive(true);
			difficultynormaltextLINE.gameObject.SetActive(false);
			PlayerPrefs.SetInt("NormalDifficultyMulti",0);
			PlayerPrefs.SetInt("HardCoreDifficultyMulti",1);
		}
		public void CreateMenuSelected()
		{
			createMenu.SetActive(true);
			lineCreate.SetActive(true);
			joinMenu.SetActive(false);
			lineJoin.SetActive(false);
			loadMenu.SetActive(false);
			lineLoad.SetActive(false);
			evolvingDetails.SetActive(true);
			errorMessage.SetActive(false);
			errorMessageLoad.SetActive(false);
		}
		
		public void JoinMenuSelected()
		{
			createMenu.SetActive(false);
			lineCreate.SetActive(false);
			joinMenu.SetActive(true);
			lineJoin.SetActive(true);
			loadMenu.SetActive(false);
			lineLoad.SetActive(false);
			evolvingDetails.SetActive(true);
			
			if (!joinMenu.activeSelf) errorMessage.SetActive(false);
			errorMessageLoad.SetActive(false);
		}
		
		public void LoadMenuSelected()
		{
			createMenu.SetActive(false);
			lineCreate.SetActive(false);
			joinMenu.SetActive(false);
			lineJoin.SetActive(false);
			loadMenu.SetActive(true);
			lineLoad.SetActive(true);
			evolvingDetails.SetActive(true);
			
			errorMessage.SetActive(false);
			if (!joinMenu.activeSelf) errorMessageLoad.SetActive(false);
		}

		public void LoadScene(string scene){
			if(scene != "")
			{
				SceneManager.LoadSceneAsync(scene);
			}
		}

		public void PlayHover(){
			hoverSound.Play();
		}
	}
}