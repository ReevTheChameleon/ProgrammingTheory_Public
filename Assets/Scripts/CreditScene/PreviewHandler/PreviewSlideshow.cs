using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Chameleon;

public class PreviewSlideshow : MonoBehaviour,
	IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
{
	[SerializeField] List<Sprite> lSprite;
	[SerializeField] float scale;
	private int indexCurrent = 0;

	public void OnPointerEnter(PointerEventData eventData){
		PreviewSlot.Instance.clearSlot();
		PreviewSlot.Instance.previewSlideshow(lSprite,scale,indexCurrent);
	}
	public void OnPointerExit(PointerEventData eventData){
		//g.transform.position = eventData.pressEventCamera.ScreenToWorldPoint(
		//	eventData.position.toVector3xy(0.0f)).newZ(0.0f);
		//Debug.Log(g.transform.position.toPreciseString());

		PreviewSlot.Instance.exitPreview();
		indexCurrent = PreviewSlot.Instance.getCurrentSlideIndex();
	}
	public void OnPointerDown(PointerEventData eventData){
		PreviewSlot.Instance.pauseSlideshow();
	}
	public void OnPointerUp(PointerEventData eventData){
		PreviewSlot.Instance.resumeSlideshow();
	}

	//[SerializeField] GameObject g;
}
