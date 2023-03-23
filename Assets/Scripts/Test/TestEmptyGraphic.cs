using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestEmptyGraphic : Graphic, IPointerEnterHandler{
	public void OnPointerEnter(PointerEventData eventData){
		Debug.Log("PointerEnter");
	}
	//protected override void OnPopulateMesh(VertexHelper vh){
	//	base.OnPopulateMesh(vh);
	//	vh.Clear();
	//}
}
