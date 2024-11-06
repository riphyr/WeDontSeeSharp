using UnityEngine;
using System.Collections;

namespace MainMenu{
    public class CheckMusicVolumes : MonoBehaviour {
        public void  Start (){
            GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
        }

        public void UpdateVolume (){
            GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
        }
    }
}