using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Chameleon;

public class EntryHoverHandler : MonoBehaviour,
	IPointerEnterHandler,IPointerExitHandler
{
	[SerializeField] TextMeshProUGUI txtName;
	[SerializeField] TextMeshProUGUI txtAuthor;
	[SerializeField] Color colorHover;
	private Color colorNormal;

	void Start(){
		colorNormal = txtName.color;
	}
	public void OnPointerEnter(PointerEventData eventData){
		/* Setting vertices color can be more performant, but the color will reset
		when text needs rebuild, which in this case occurs when we change overflowMode.
		It is possible to do, though, but the resulting code is less clean and
		the performance gain is about 0.2ms for that frame only, which doesn't worth it
		in my opinion. */
		//txtName.setVerticesColor(colorHover);
		//txtAuthor.setVerticesColor(colorHover);
		txtName.color = colorHover;
		txtAuthor.color = colorHover;
	}
	public void OnPointerExit(PointerEventData eventData){
		//txtName.setVerticesColor(colorNormal);
		//txtAuthor.setVerticesColor(colorNormal);
		txtName.color = colorNormal;
		txtAuthor.color = colorNormal;
	}
}

