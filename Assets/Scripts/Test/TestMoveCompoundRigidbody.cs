using UnityEngine;

public class TestMoveCompoundRigidbody : MonoBehaviour{
	Rigidbody rb;
	public float speed;
	
	void Awake(){
		rb = GetComponent<Rigidbody>();
	}
	void FixedUpdate(){
		rb.position += speed*Time.fixedDeltaTime*Vector3.forward;
	}
}
