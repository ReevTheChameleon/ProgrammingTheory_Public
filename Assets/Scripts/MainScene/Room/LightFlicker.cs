using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour{
	[SerializeField] float amplitude;
	[SerializeField] float speed;
	[SerializeField] LightFlicker syncLightFlicker; //for syncing to other LightFlicker
	Light lgt;
	private float baseIntensity;
	private Vector2 v2OffsetPerlin;

	void Awake(){
		lgt = GetComponent<Light>();
		baseIntensity = lgt.intensity;
		/* This allows for flexible syncing where different LightFlicker
		can sync to each other across GameObjects, if that happens to be needed. */
		v2OffsetPerlin =
			syncLightFlicker ?
			syncLightFlicker.v2OffsetPerlin :
			new Vector2(Random.Range(0.0f,5.0f),Random.Range(0.0f,5.0f))
		;
	}
	void Update(){
		lgt.intensity = baseIntensity -
			amplitude*Mathf.PerlinNoise(v2OffsetPerlin.x,v2OffsetPerlin.y);
		v2OffsetPerlin.y += speed*Time.deltaTime;
	}

	#if UNITY_EDITOR
	void OnValidate(){
		if(syncLightFlicker){
			speed = syncLightFlicker.speed;}
	}
	#endif
}
