using UnityEngine;
using Chameleon;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; //for test, remove later

public class KeyManager : LoneMonoBehaviour<KeyManager>{
	[SerializeField] GameObject gUI;
	[SerializeField] Image imgKeyIcon;
	[SerializeField][GrayOnPlay] TextMeshProUGUI txtCount;
	[SerializeField][GrayOnPlay] float scaleFontBump;
	[SerializeField][GrayOnPlay] float durationFontBump;
	[SerializeField] Color colorMinus;
	private LoneCoroutine routineChangeKeyCount;
	private Color colorNormal;

	[SerializeField] Image imgKeyIconPick;
	[SerializeField] float durationTweenIconPick;
	private LoneCoroutine routineTweenIconPick;
	private TweenRoutineUnit<Vector3> subitrTweenIconPick;
	
	public int KeyCount{get; private set;} = 0;
	 
	void Start(){
		colorNormal = txtCount.color;
		routineChangeKeyCount = new LoneCoroutine(this,
			txtCount.sinBumpFontSize(
				scaleFontBump,
				durationFontBump,
				dOnDone: (float t) => txtCount.color=colorNormal
			)
		);
		subitrTweenIconPick = imgKeyIconPick.transform.tweenPosition(
			Vector3.zero,
			imgKeyIcon.transform.position,
			durationTweenIconPick,
			dOnDone: (float t) => {imgKeyIconPick.gameObject.SetActive(false);}
		);
		routineTweenIconPick = new LoneCoroutine(this,subitrTweenIconPick);
	}
	public void addKey(){
		gUI.SetActive(true);
		txtCount.text = "x"+ ++KeyCount;
		routineChangeKeyCount.restart();
		routineChangeKeyCount.resume();
	}
	public bool removeKey(){
		if(KeyCount <= 0)
			return false;
		txtCount.text = "x"+ --KeyCount;
		txtCount.color = colorMinus;
		routineChangeKeyCount.restart();
		routineChangeKeyCount.resume();
		return true;
	}
	public WaitLoneCoroutine tweenIconKeyPick(Vector3 vWorldPos,bool bCollect){ //bCollect specifies direction
		//Vector3 vIconPickWorldPos = Camera.main.WorldToScreenPoint(vWorldPos).newZ(0.0f);
		//Debug.Log(vIconPickWorldPos);
		/* Although z doesn't seem to matter for overlay Canvas (we assume icon is on such),
		if it is outside some range, it will not show up, so better to play safe. */
		subitrTweenIconPick.bReverse = !bCollect;
		subitrTweenIconPick.reset(
			Camera.main.WorldToScreenPoint(vWorldPos).newZ(0.0f),
			imgKeyIcon.transform.position
		);
		imgKeyIconPick.gameObject.SetActive(true);
		return routineTweenIconPick.resume();
	}
	
	#if ENABLE_PROFILER
	void Update(){
		if(Keyboard.current.upArrowKey.wasPressedThisFrame)
			addKey();
		if(Keyboard.current.downArrowKey.wasPressedThisFrame)
			removeKey();
		//Debug.Log(routineTweenIconPick.IsRunning);
	}
	#endif
}
