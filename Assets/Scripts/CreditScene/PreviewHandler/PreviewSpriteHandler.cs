using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewSpriteHandler : MonoBehaviour,
	IPointerEnterHandler,IPointerExitHandler
{
	[SerializeField] Sprite sprite;
	[SerializeField] float scale;
	[SerializeField] Color colorBackground = Color.clear;
	[SerializeField] Color colorTint = Color.white;

	public void OnPointerEnter(PointerEventData eventData){
		PreviewSlot.Instance.previewSprite(sprite,scale,colorBackground,colorTint);
	}
	public void OnPointerExit(PointerEventData eventData){
		PreviewSlot.Instance.exitPreview();
	}
}
