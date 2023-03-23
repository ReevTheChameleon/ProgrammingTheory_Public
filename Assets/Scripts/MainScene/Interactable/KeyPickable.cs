using UnityEngine;
using Chameleon;
using System.Collections;

public class KeyPickable : PickableInspectable{
	MeshRenderer meshRenderer;
	LoneCoroutine routineTweenIconPick = new LoneCoroutine();

	protected override void Awake(){
		meshRenderer = GetComponentInChildren<MeshRenderer>();
		base.Awake();
	}
	protected override void OnDisable(){
		base.OnDisable();
		meshRenderer.enabled = true;
	}
	public override void onPicked(){
		base.onPicked();
		routineTweenIconPick.start(this,rfPickSequence());
	}
	private IEnumerator rfPickSequence(){
		meshRenderer.enabled = false;
		yield return KeyManager.Instance.tweenIconKeyPick(transform.position,true);
		KeyManager.Instance.addKey();
	}
}
