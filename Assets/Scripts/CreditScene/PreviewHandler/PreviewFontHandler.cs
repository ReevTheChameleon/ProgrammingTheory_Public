using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PreviewFontHandler : MonoBehaviour,
	IPointerEnterHandler,IPointerExitHandler
{
	[SerializeField] TMP_FontAsset font;
	[SerializeField] float fontSize;
	[SerializeField] Color colorTint;

	public void OnPointerEnter(PointerEventData eventData){
		PreviewSlot.Instance.clearSlot();
		PreviewSlot.Instance.previewText(font,fontSize,colorTint);
	}
	public void OnPointerExit(PointerEventData eventData){
		PreviewSlot.Instance.exitPreview();
	}
}
