using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewBgmHandler : MonoBehaviour,
	IPointerEnterHandler,IPointerExitHandler
{
	[SerializeField] AudioPrefab apfBgm;

	public void OnPointerEnter(PointerEventData eventData){
		PreviewSlot.Instance.previewAudioSprite();
		if(BgmPlayer.Instance.ClipCurrent != apfBgm.audioClip){
			BgmPlayer.Instance.playBgm(apfBgm);}
	}
	public void OnPointerExit(PointerEventData eventData){
		PreviewSlot.Instance.exitPreview();
	}
}
