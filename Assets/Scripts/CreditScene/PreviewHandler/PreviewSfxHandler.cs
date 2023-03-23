using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewSfxHandler : MonoBehaviour,
	IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler
{
	[Tooltip("Assign to aus for continuous sound effects, and leave it empty for one-time")]
	[SerializeField] AudioSource aus;
	[SerializeField] AudioPrefab sfxpf;

	public void OnPointerEnter(PointerEventData eventData){
		PreviewSlot.Instance.previewAudioSprite();
		if(aus){
			aus.Play();}
		else{
			SfxPlayer.Instance.play(sfxpf);}
	}
	public void OnPointerExit(PointerEventData eventData){
		if(aus){
			aus.Stop();}
		PreviewSlot.Instance.exitPreview();
	}
	public void OnPointerDown(PointerEventData eventData){
		if(!aus){
			SfxPlayer.Instance.play(sfxpf); }
	}
}
