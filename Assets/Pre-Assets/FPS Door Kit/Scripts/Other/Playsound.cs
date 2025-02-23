
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;

namespace EnivStudios
{
	public class Playsound : MonoBehaviour
	{
		public void Clicky()
		{
			GetComponent<AudioSource>().Play();
		}
	}
}
