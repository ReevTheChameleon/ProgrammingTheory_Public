using UnityEngine;

public class RotateY : MonoBehaviour{
	[SerializeField] float speed = -60.0f; //deg/sec
	void Update(){
		transform.Rotate(0.0f,speed*Time.deltaTime,0.0f,Space.World);
	}
}
