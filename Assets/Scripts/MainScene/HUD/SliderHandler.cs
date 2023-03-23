using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Chameleon;

[RequireComponent(typeof(Image))]
public class SliderHandler : MonoBehaviour, 
	IPointerEnterHandler,IPointerExitHandler,
	IDragHandler,IPointerDownHandler,IPointerUpHandler
{
	[SerializeField] Color colorNormal;
	[SerializeField] Color colorHover;
	[SerializeField] Color colorClick;
	[SerializeField] RectTransform rtBackground;
	[SerializeField] RectTransform rtFill;
	[SerializeField] AudioPrefab sfxpfHover;
	[SerializeField] AudioPrefab sfxpfRelease;

	public delegate void DOnFractionChange(float fraction);
	private DOnFractionChange dOnFractionChange;
	private bool bDown;
	private bool bInside;
	Image image;

	void Awake(){
		image = GetComponent<Image>();
		this.delayCallEndOfFrame(()=>{Fraction=Fraction;});
	}
	public float Fraction{
		get{return rtFill.rect.width/rtBackground.rect.width;}
		set{rtFill.setWidth(value * rtBackground.rect.width);}
	}
	public void setOnValueChangeAction(DOnFractionChange action){
		dOnFractionChange = action;
	}
	public void OnPointerEnter(PointerEventData eventData){
		bInside = true;
		if(!bDown){
			SfxPlayer.Instance.play(sfxpfHover);
			image.color = colorHover;
		}
	}
	public void OnPointerExit(PointerEventData eventData){
		bInside = false;
		if(!bDown){
			image.color = colorNormal;}
	}
	public void OnPointerDown(PointerEventData eventData){
		bDown = true;
		image.color = colorClick;
		SfxPlayer.Instance.play(sfxpfRelease);
	}
	public void OnPointerUp(PointerEventData eventData){
		bDown = false;
		image.color = bInside ? colorHover : colorNormal;
		SfxPlayer.Instance.play(sfxpfRelease);
	}
	public void OnDrag(PointerEventData eventData){
		updateDrag(eventData.position,eventData.pressEventCamera);
	}
	//public void OnInitializePotentialDrag(PointerEventData eventData) {
	//	updateDrag(eventData.position,eventData.pressEventCamera);
	//}
	private void updateDrag(Vector2 v2ScreenPos,Camera camera){
		Vector2 v2Local;
		//Credit: PGJ, UF
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			rtBackground,v2ScreenPos,camera,out v2Local);
		float fraction = Mathf.Clamp01(v2Local.x/rtBackground.rect.width);
		Fraction = fraction;
		dOnFractionChange?.Invoke(fraction);
	}
}
