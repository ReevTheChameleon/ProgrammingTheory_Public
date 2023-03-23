/* NOT USED: Because tweaking settings so the lights are culled on "IgnoreLight" layer
is sufficient to simulate "light off" result on the walls. */

using UnityEngine;

public class LightToggler : MonoBehaviour{
	[SerializeField][HideInInspector] Light[] aLight;
	[SerializeField] bool bOn = true;

	void Awake(){
		aLight = GetComponentsInChildren<Light>();
	}
	public void setLight(bool bOn){
		for(int i=0; i<aLight.Length; ++i){
			aLight[i].enabled = bOn; }
		this.bOn = bOn;
	}
	public bool isLightOn(){
		return bOn;
	}

	#if UNITY_EDITOR
	void OnValidate(){
		Awake();
		setLight(bOn);
	}
	#endif
}
