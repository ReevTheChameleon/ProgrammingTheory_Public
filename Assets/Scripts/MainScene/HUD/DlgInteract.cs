using UnityEngine;
using Chameleon;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class DlgInteract : DlgTwoButton{
	[SerializeField] float durationDlgTween;
	//protected TweenRoutineUnit subitrTweenDlg;
	private TweenRoutineUnit<Vector2> subitrTweenDlg;

	void Start(){
		subitrTweenDlg = ((RectTransform)transform).tweenAnchoredPosition(
			Vector2.zero,Vector2.zero,durationDlgTween);
	}
	public TweenRoutineUnit<Vector2> prepareItrTweenDlg(bool bReverse){
		RectTransform rt = (RectTransform)transform;
		Vector2 v2DlgPosEnd = subitrTweenDlg.Start;
		v2DlgPosEnd.x -= rt.rect.width;
		subitrTweenDlg.bReverse = bReverse;
		subitrTweenDlg.reset(subitrTweenDlg.Start,v2DlgPosEnd);
		return subitrTweenDlg;
	}
}
