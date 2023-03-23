using UnityEngine;
using UnityEngine.EventSystems;
using Chameleon;

public class PreviewGameObjectHandler : MonoBehaviour,
	IPointerEnterHandler,IPointerExitHandler
{
	[SerializeField] GameObject gPreview;
	[SerializeField] float scale;
	[SerializeField] Vector2 v2Offset;
	[SerializeField] AnimationClip animClip;
	private AnimationPlayer animPlayer;

	void Awake(){
		animPlayer = gPreview.GetComponentInChildren<AnimationPlayer>();
	}
	public void OnPointerEnter(PointerEventData eventData){
		PreviewSlot.Instance.clearSlot();
		gPreview.SetActive(true);
		gPreview.transform.position =
			PreviewSlot.Instance.transform.position + (Vector3)v2Offset;
		gPreview.transform.localScale = new Vector3(scale,scale,scale);
		if(animClip){
			animPlayer.play(animClip);}
	}
	public void OnPointerExit(PointerEventData eventData){
		if(animClip){
			animPlayer.stopLayer();}
		gPreview.SetActive(false);
		PreviewSlot.Instance.exitPreview();
	}
}
