/*
Usage:
Attach this script to the Canvas GameObject and call its SetActiveCanvas
instead of SetActive the entire Canvas GameObject.
Rationale:
Disabling Canvas Component causes it to discard relevant information.
When Canvas is re-enabled, it must allocate a big chunk of memory and recalculate
everything again. This script is the workaround to avoid that (Credit idea: JohnTube, UF).
Note: It seems that disabling the last active Graphic on the Canvas also produces
the same effect as disabling the Canvas itself, so disabling UI GameObject also
does not work if you only have one UI.
 */

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Chameleon{

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
public class CanvasToggler : MonoBehaviour{
	[SerializeField] bool bActiveCanvas = true;
	CanvasGroup canvasGroup;
	GraphicRaycaster graphicRaycaster;

	void Awake(){
		canvasGroup = GetComponent<CanvasGroup>();
		graphicRaycaster = GetComponent<GraphicRaycaster>();
	}
	void Start(){
		setActiveCanvas(bActiveCanvas);
	}
	public void setActiveCanvas(bool bActive){
		if(bActive){
			canvasGroup.alpha = 1.0f;
			if(graphicRaycaster){
				graphicRaycaster.enabled = true;}
		}
		else{ //!bActive
			if(graphicRaycaster){
				graphicRaycaster.enabled = false;}
			canvasGroup.alpha = 0.0f;
		}
		bActiveCanvas = bActive;
	}
	#if UNITY_EDITOR
	void OnValidate(){
		if(!EditorApplication.isPlaying){
			Awake();
			EditorApplication.delayCall += () => {
				if(!canvasGroup ){
					return;}
				setActiveCanvas(bActiveCanvas);
			};
		}
		else if(canvasGroup){ //avoid OnValidate when change Play Mode
			setActiveCanvas(bActiveCanvas); }
	}
	#endif
}

} //end namespace Chameleon
