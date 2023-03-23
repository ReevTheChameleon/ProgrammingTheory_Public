using UnityEngine;
using TMPro;
using Chameleon;

[RequireComponent(typeof(TMP_Text))]
public class TextCycleColor : MonoBehaviour{
	[SerializeField] Gradient gradient;
	[SerializeField] float speed;
	TMP_Text txtTarget;
	private float samplePoint = 0.0f;
	private Color colorStart;

	void Awake(){
		txtTarget = GetComponent<TMP_Text>();
	}
	void Start(){
		colorStart = txtTarget.color;
	}
	void Update(){
		samplePoint = (samplePoint+speed*Time.deltaTime) % 1.0f;
		txtTarget.setVerticesColor(gradient.Evaluate(samplePoint));
	}
	void OnEnable(){
		samplePoint = 0.0f;
	}
	void OnDisable(){
		txtTarget.color = colorStart;
	}
}
