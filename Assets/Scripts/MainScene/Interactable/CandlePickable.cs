using UnityEngine;
using Chameleon;
using System.Collections;

public class CandlePickable : PickableInspectable{
	[SerializeField] float lightAmount;
	LoneCoroutine routineTweenIconPick = new LoneCoroutine();

	protected override void OnEnable(){
		/* In fact it would be great if a candle would also set bLurking of
		Interactable in vicinity to false, but the "vicinity" range is hard
		to determine because although the object is within Light range,
		it may still be perceived as "not well-lit", so we make it such that
		only the candle pile itself is visible. */
		bLurking = false;
		for(int i=0; i<transform.childCount; ++i){
			transform.GetChild(i).gameObject.SetActive(true);}
	}
	public override void onPicked(){
		base.onPicked();
		routineTweenIconPick.start(this,rfPickSequence());
	}
	private IEnumerator rfPickSequence(){
		//for(int i=0; i<transform.childCount; ++i){
		//	transform.GetChild(i).gameObject.SetActive(false);}
		yield return CandleManager.Instance.tweenIconCandlePick(transform.position);
		CandleManager.Instance.addLight(lightAmount);
	}
}
