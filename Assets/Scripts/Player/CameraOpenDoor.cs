using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOpenDoor : MonoBehaviour 
{
	public float DistanceOpen = 3;
	public GameObject textOpenDoor, textOpenDoorFrame;

	void Start()
	{
	}
	
	void Update () 
	{
		RaycastHit hit;
		
		if (Physics.Raycast (transform.position, transform.forward, out hit, DistanceOpen)) 
		{ 
			if (hit.transform.GetComponent<DoorScript.Door> ()) 
			{
				textOpenDoor.SetActive(true);
				textOpenDoorFrame.SetActive(true);
				string interactKey = PlayerPrefs.GetString("Interact", "None");
				if (Input.GetKeyDown(GetKeyCodeFromString(interactKey)))
					hit.transform.GetComponent<DoorScript.Door> ().OpenDoor();
			}
			else
			{
				textOpenDoor.SetActive(false);
				textOpenDoorFrame.SetActive(false);
			}
		}
		else
		{
			textOpenDoor.SetActive(false);
			textOpenDoorFrame.SetActive(false);
		}
	}
	
	private KeyCode GetKeyCodeFromString(string key)
	{
		return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
	}
}
