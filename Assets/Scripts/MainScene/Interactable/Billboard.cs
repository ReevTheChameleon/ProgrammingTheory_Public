using UnityEngine;
using Chameleon;

public class Billboard : MonoBehaviour{
	private Transform tCamera;

	void Awake(){
		tCamera = Camera.main.transform;
	}
	void LateUpdate(){
		transform.lookDirection(-tCamera.forward,tCamera.up);
	}
}
