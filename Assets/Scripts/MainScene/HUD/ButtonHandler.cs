using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Chameleon;

/* There is a problem where if you press on one button and drag the mouse over
to another button, the latter will be highlighted. I have found this quite late
in the development, and it is not very important, so I decide to let it slide
for this project.
Note: Unity's default Buttons also have the same problem. */
[RequireComponent(typeof(Image))]
public class ButtonHandler : MonoBehaviour,
	IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler,
	IPointerDownHandler,IPointerUpHandler
{
	[SerializeField] Color colorNormal;
	[SerializeField] Color colorHover;
	[SerializeField] Color colorClick;
	[SerializeField] AudioPrefab sfxpfDataHover;
	[SerializeField] AudioPrefab sfxpfDataClick;

	private Action dOnClick;
	private bool bDown = false;
	AudioSource ausUI;
	Image image;

	protected virtual void Awake(){
		image = GetComponent<Image>();
		ausUI = GetComponentInParent<AudioSource>();
	}
	public void setOnClickAction(Action action){
		dOnClick = action;
	}
	public void OnPointerClick(PointerEventData eventData){
		dOnClick?.Invoke();
	}
	public void OnPointerEnter(PointerEventData eventData){
		image.color = bDown ? colorClick : colorHover;
		SfxPlayer.Instance.play(sfxpfDataHover);
	}
	public void OnPointerExit(PointerEventData eventData){
		image.color = colorNormal;
	}
	public void OnPointerDown(PointerEventData eventData){
		image.color = colorClick;
		SfxPlayer.Instance.play(sfxpfDataClick);
		bDown = true;
		#if UNITY_WEBGL
		/* Because locking cursor in WebGL merely sends request to the browser,
		and require one user action before really locking. Since this game relies on
		this a lot (otherwise mouse look will fail), we must make sure lockState is done
		at least after user click any button.
		This is done by making request in OnPointerDown so when user release the mouse
		it counts as user action and does the lock. */
		CursorController.ShowCursor = false;
		OnPointerClick(eventData);
		#endif
	}
	public void OnPointerUp(PointerEventData eventData){
		image.color = colorNormal;
		bDown = false;
	}
}
