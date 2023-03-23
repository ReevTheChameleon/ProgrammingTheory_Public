using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewMaterialHandler : MonoBehaviour,
	IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
{
	[SerializeField] Material material;
	[SerializeField] Vector2 uv;

	public void OnPointerEnter(PointerEventData eventData){
		PreviewSlot.Instance.previewMaterial(material,uv);
	}
	public void OnPointerExit(PointerEventData eventData){
		PreviewSlot.Instance.exitPreview();
	}
	public void OnPointerDown(PointerEventData eventData){
		PreviewSlot.Instance.setPreviewMaterialRotate(false);
	}
	public void OnPointerUp(PointerEventData eventData){
		PreviewSlot.Instance.setPreviewMaterialRotate(true);
	}
}
