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
        
        [Header("HIGHLIGHT")] 
        public GameObject lineCreate;
        public GameObject lineJoin;
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
			evolvingDetails.SetActive(false);
			
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
			evolvingDetails.SetActive(true);
		}
		
		public void JoinMenuSelected()
		{
			createMenu.SetActive(false);
			lineCreate.SetActive(false);
			joinMenu.SetActive(true);
			lineJoin.SetActive(true);
			evolvingDetails.SetActive(true);
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