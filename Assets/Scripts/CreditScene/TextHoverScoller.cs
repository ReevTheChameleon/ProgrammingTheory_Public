using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Chameleon;
using UnityEngine.UI;

public class TextHoverScoller : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] float speedScroll;
	TextMeshProUGUI txt;
	private float widthSlide;
	private LoneCoroutine routineScroll;

	void Awake(){
		txt = GetComponentInChildren<TextMeshProUGUI>();
	}
	void Start(){
		/* For an element in layout groups, calling this in Start() will return zero
		because layout groups need at least 1 frame to update (Credit: DiegoSLTS, UA) */
		this.delayCallEndOfFrame(init);
	}
	private void init(){
		txt.overflowMode = TextOverflowModes.Overflow;
		txt.ForceMeshUpdate();
		widthSlide = txt.textBounds.size.x - ((RectTransform)transform).rect.width;
		//Debug.Log(txt.textBounds.size.x);
		//Debug.Log(((RectTransform)transform).rect.width);
		txt.overflowMode = TextOverflowModes.Ellipsis;
		/* This reduces unnecessary burden to GraphicRaycaster */
		if(widthSlide <= 0){
			GetComponent<Graphic>().raycastTarget = false;}
		routineScroll = new LoneCoroutine(this,
			txt.rectTransform.tweenAnchoredPosition(
				Vector2.zero,
				new Vector2(-widthSlide,0),
				widthSlide/speedScroll
			)
		);
	}
	public void OnPointerEnter(PointerEventData eventData){
		//if(widthSlide > 0){
			txt.overflowMode = TextOverflowModes.Overflow;
			routineScroll.resume();
		//}
	}
	public void OnPointerExit(PointerEventData eventData){
		//if(widthSlide > 0){
			routineScroll.stop();
			routineScroll.Itr.Reset();
			txt.overflowMode = TextOverflowModes.Ellipsis;
		//}
	}
}
