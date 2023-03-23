using UnityEngine;
using UnityEngine.InputSystem;
using Chameleon;

[RequireComponent(typeof(PlayerInput))]
public class TestExecutionOrder : MonoBehaviour{
	public InputActionID actionId;
	private int frame = 0;

	void OnEnable(){
		GetComponent<PlayerInput>().actions[actionId].performed += onInput;
	}
	void OnDisable(){
		GetComponent<PlayerInput>().actions[actionId].performed -= onInput;
	}
	void onInput(InputAction.CallbackContext x){
		Debug.LogWarning("Frame "+frame+" Input");
	}
	void Update(){
		Debug.Log("Frame "+frame+" Update");
	}
	void FixedUpdate(){
		Debug.Log("Frame "+frame+" FixedUpdate");
	}
	void LateUpdate(){
		Debug.Log("Frame "+frame+" LateUpdate");
		++frame;
	}
}
