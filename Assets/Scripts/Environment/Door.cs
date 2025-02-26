using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace DoorScript
{
	[RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(PhotonView))]
	public class Door : MonoBehaviour, IPunObservable
	{
		public bool open;
		public float smooth = 1.0f;
		float DoorOpenAngle = -90.0f;
	    float DoorCloseAngle = 0.0f;
		public AudioSource asource;
		public AudioClip openDoor,closeDoor;
		private PhotonView view;
		
		void Start () 
		{
			asource = GetComponent<AudioSource> ();
			view = GetComponent<PhotonView>();
		}
		
		void Update () 
		{
			if (open)
			{
	            var target = Quaternion.Euler (0, DoorOpenAngle, 0);
	            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * 5 * smooth);
		
			}
			else
			{
	            var target1= Quaternion.Euler (0, DoorCloseAngle, 0);
	            transform.localRotation = Quaternion.Slerp(transform.localRotation, target1, Time.deltaTime * 5 * smooth);
		
			}  
		}

		public void OpenDoor()
		{
			if (view.IsMine)
			{
				open = !open;
				asource.clip = open ? openDoor : closeDoor;
				asource.Play();
			}
		}
		
		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.IsWriting)
			{
				stream.SendNext(open);
			}
			else
			{
				open = (bool)stream.ReceiveNext();
			}
		}
	}
}