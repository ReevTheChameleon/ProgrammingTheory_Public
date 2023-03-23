using UnityEngine;

public class TestPerlin : MonoBehaviour{
	[SerializeField] float amplitude;
	[SerializeField] float speed;
	private float offsetPerlin;

	void Awake(){
		offsetPerlin = Random.Range(0.0f,1.0f);
	}
	void Update(){
		float f = amplitude*(Mathf.PerlinNoise(offsetPerlin,offsetPerlin)-0.5f);
		Debug.Log(f);
		transform.position = new Vector3(0,f,0);
		offsetPerlin += speed*Time.deltaTime;
	}
}
