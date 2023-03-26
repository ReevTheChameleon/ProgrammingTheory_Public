using UnityEngine;
using Chameleon;
using System.Collections;
using TMPro;

public class CandleManager : LoneMonoBehaviour<CandleManager>{
	[Header("HUD")]
	[SerializeField] GameObject gUI;
	[SerializeField] RectTransform rtBack;
	[SerializeField] RectTransform rtMid;
	[SerializeField] RectTransform rtFront;
	[SerializeField] float durationTweenBar;
	[SerializeField] float durationSuspend;
	[SerializeField] float rateLightConsumption; //fraction/s
	[SerializeField] GameObject gIconFlame;
	[SerializeField] TextMeshProUGUI txtUse;
	[SerializeField] string sPressToLit;
	[SerializeField] string sPressToUnlit;
	private LoneCoroutine routineAddLight = new LoneCoroutine();
	private TweenRoutineUnit<float> subitrTweenLightBarWidth;
	private WaitForSeconds waitSuspend;


	[Header("Icon Pick")]
	[SerializeField] Transform tIconCandle;
	[SerializeField] Transform tIconCandlePick;
	[SerializeField] float durationTweenIconPick;
	private LoneCoroutine routineTweenIconCandlePick;
	private TweenRoutineUnit<Vector3> subitrTweenIconCandlePick;
	
	[Header("Candle")]
	[SerializeField] GameObject gCandle;
	[SerializeField] IKTwoBone rigCandleArm;
	[SerializeField] RigFixed rigCandleFinger;
	[SerializeField][GrayOnPlay] float durationRaiseCandle;
	[SerializeField] AudioPrefab sfxpfLightCandle;
	private LoneCoroutine routineRaiseCandle;
	private float fullWeightRigCandleArm; 

	public float Fraction{get; private set;} = 0.0f;

	void Start(){
		waitSuspend = new WaitForSeconds(durationSuspend);
		subitrTweenLightBarWidth = rtFront.tweenWidth(0.0f,0.0f,durationTweenBar);
		subitrTweenIconCandlePick = tIconCandlePick.tweenPosition(
			Vector3.zero,
			tIconCandle.position,
			durationTweenIconPick,
			dOnDone: (float t) => {tIconCandlePick.gameObject.SetActive(false);}
		);
		routineTweenIconCandlePick = new LoneCoroutine(this,subitrTweenIconCandlePick);
		rtFront.setWidth(0.0f);
		this.enabled = false;
		//set full weight in rig component, and it will be fixed once game start
		fullWeightRigCandleArm = rigCandleArm.weight;
		routineRaiseCandle = new LoneCoroutine(this,
			new TweenRoutineUnit(
				(float t) => {rigCandleArm.weight = t*fullWeightRigCandleArm;},
				durationRaiseCandle,
				dOnDone: (float t) => {if(t<=0.0f) rigCandleArm.enabled = false;}
			)
		);
		rigCandleArm.weight = 0.0f;
		setCandleLight(false);
	}
	public void addLight(float fraction){
		gUI.SetActive(true);
		rtFront.setWidth(rtBack.rect.width*Fraction);
		Fraction = Mathf.Clamp01(Fraction+fraction);
		txtUse.gameObject.SetActive(true);
		this.enabled = true;
		routineAddLight.start(this,rfIncreaseLightBar());
	}
	public void addLightImmediate(float fraction){
		gUI.SetActive(true);
		Fraction = Mathf.Clamp01(Fraction+fraction);
		rtFront.setWidth(rtBack.rect.width*Fraction);
	}
	private IEnumerator rfIncreaseLightBar(){
		rtMid.gameObject.SetActive(true);
		rtMid.setWidth(rtBack.rect.width * Fraction);
		yield return waitSuspend;
		subitrTweenLightBarWidth.reset(rtFront.rect.width,rtMid.rect.width);
		while(subitrTweenLightBarWidth.MoveNext()){
			if(rtFront.rect.width >= rtMid.rect.width){
				 /* in case where candle is being used, so mid bar is decreasing,
				 and may be shorter than original subitr.End */
				rtFront.setWidth(rtMid.rect.width);
				break;
			}
			yield return subitrTweenLightBarWidth.Current;
		}
		rtMid.gameObject.SetActive(false);
	}
	public bool IsLit{get; private set;}
	public bool toggleCandleLight(){ //return state after toggle
		return setCandleLight(!IsLit);
	}
	private bool setCandleLight(bool bLit){
		if(bLit && Fraction <= 0.0f){
			return false;}
		IsLit = bLit;
		this.enabled = true;
		gCandle.SetActive(bLit);
		gIconFlame.SetActive(bLit);
		txtUse.text = bLit ? sPressToUnlit : sPressToLit;
		rigCandleArm.enabled = true;
		routineRaiseCandle.getItr<TweenRoutineUnit>().bReverse = !bLit;
		routineRaiseCandle.resume();
		if(bLit){
			SfxPlayer.Instance.play(sfxpfLightCandle); }
		rigCandleFinger.enabled = bLit;
		return bLit;
	}
	public WaitLoneCoroutine tweenIconCandlePick(Vector3 vWorldPos){
		subitrTweenIconCandlePick.reset(
			Camera.main.WorldToScreenPoint(vWorldPos).newZ(0.0f),
			tIconCandle.position
		);
		tIconCandlePick.gameObject.SetActive(true);
		return routineTweenIconCandlePick.resume();
	}
	void Update(){
		if(IsLit){
			if(PlayerController.Instance.InputMode!=eInputMode.MainGameplay ||
				PlayerController.Instance.IsPause){
				return;}
			Fraction = Mathf.Clamp01(Fraction-rateLightConsumption*Time.deltaTime); //can use Mathf.Min
			if(routineAddLight.IsRunning){
				rtMid.setWidth(rtBack.rect.width*Fraction);}
			else{
				rtFront.setWidth(rtBack.rect.width*Fraction);
				if(Fraction <= 0.0f){
					setCandleLight(false);
					txtUse.gameObject.SetActive(false);
				}
			}
		}
		else if(!routineAddLight.IsRunning){
			this.enabled = false;}
	}
}
