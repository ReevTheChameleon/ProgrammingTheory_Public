using UnityEngine;
using Chameleon;
using System.Collections;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DoorLocked : OptionInspectable{
	[SerializeField] AnimationClip clipUnlock;
	[SerializeField] float durationWaitUnlocked;
	[SerializeField] float durationDoorLift;
	[SerializeField] float distanceLift;
	[SerializeField] float distanceUnlock;
	[SerializeField] float durationTurn;

	[SerializeField] float camDistanceUnlock;
	[SerializeField] Vector2 rangeEulerXCamUnlock;
	[SerializeField] Transform tKeyHole;

	[SerializeField] AudioPrefab sfxpfUnlock;
	[SerializeField] AudioPrefab sfxpfSlide;
	[SerializeField] Collider cBlocker;
	AnimationPlayer animPlayerUnlock;

	private TweenRoutineUnit<Quaternion> subitrTurnPlayer;
	private TweenRoutineUnit<Vector3> subitrDoorLift;

	protected override void Awake(){
		base.Awake();
		animPlayerUnlock = GetComponentInChildren<AnimationPlayer>();
		subitrTurnPlayer = PlayerController.Instance.transform.tweenRotation(
			Quaternion.identity,Quaternion.identity,durationTurn);
		subitrDoorLift = transform.tweenPosition(
			Vector3.zero,
			Vector3.zero,
			//transform.position,
			//transform.position + new Vector3(0.0f,distanceLift,0.0f),
			durationDoorLift
		);
	}
	public override void onInteracted(){
		switch(inspectionState){
			case eInspectionState.None:
				SfxPlayer.Instance.play(sfxpfInspect);
				if(KeyManager.Instance.KeyCount > 0)
					routineInteract.start(this,rfStartOptionInspcetSequence());
				else
					routineInteract.start(this,rfStartInspectSequence());
				break;
			case eInspectionState.FooterTexting:
				FooterManager.Instance.stepFooter();
				break;
			case eInspectionState.Suspended:
				if(KeyManager.Instance.KeyCount == 0)
					routineInteract.start(this,rfEndInspectSequence());
				break;
			case eInspectionState.EndSequence:
				break; //do nothing
		}
	}
	protected override void onOptionAction(){
		//KeyManager.Instance.removeKey();
		inspectionState = eInspectionState.EndSequence;
		routineInteract.start(this,rfUnlockSequence());
	}
	private IEnumerator rfUnlockSequence(){
		CursorController.ShowCursor = false;
		DlgInteract dlgFooter = SceneMainManager.Instance.DlgFooter;
		CamInfo camInfoStart = new CamInfo(subitrPanCamera.Start);
		subitrPanCamera.Start.vCamTarget = subitrPanCamera.End.vCamTarget;
		subitrPanCamera.Start.qCamTarget = tCamTarget.rotation;
		Vector3 eulerCamEnd = tCamTarget.localEulerAngles;
		subitrPanCamera.End.qCamTarget = Quaternion.Euler(eulerCamEnd.newX(
			MathfExtension.clamp(Mathf.DeltaAngle(0.0f,eulerCamEnd.x),rangeEulerXCamUnlock))
		);
		subitrPanCamera.Start.camDistance = cameraControl.targetCameraDistance;
		subitrPanCamera.End.camDistance = camDistanceUnlock;
		subitrPanCamera.bReverse = false;
		subitrPanCamera.Reset();
		yield return new ParallelEnumerator(this,
			dlgFooter.prepareItrTweenDlg(true),
			subitrPanCamera
		);

		dlgFooter.close(true);
		PlayerController playerController = PlayerController.Instance;
		Vector3 vWalkTarget =
			(transform.position +
			Quaternion.FromToRotation(Vector3.forward,transform.forward) *
				playerController.VOpenOffset
			).newY(playerController.transform.position.y)
		;
		//PlayerController.eCutsceneMotion cutsceneMotion;
		//float angle = Vector3.SignedAngle(
		//	playerController.transform.forward,transform.forward,Vector3.up);
		//if(angle < -30.0f) cutsceneMotion = PlayerController.eCutsceneMotion.SlideRight;
		//else if(angle > 10.0f) cutsceneMotion = PlayerController.eCutsceneMotion.SlideLeft;
		//else{
		//	cutsceneMotion = Vector3.Dot(
		//		playerController.transform.forward,
		//		vWalkTarget - playerController.transform.position
		//	) >= 0.0f ?
		//	PlayerController.eCutsceneMotion.Walk :
		//	PlayerController.eCutsceneMotion.WalkBackward;
		//}
		IEnumerator subitrWalkToward = playerController.rfWalkToward(
			vWalkTarget,
			//cutsceneMotion
			Vector3.Cross(playerController.transform.forward,transform.forward).y < 0.0f ?
				PlayerController.eCutsceneMotion.SlideRight :
				PlayerController.eCutsceneMotion.SlideLeft
		);
		while(subitrWalkToward.MoveNext()){
			playerController.transform.setEulerY(
				(transform.position-playerController.transform.position).eulerAngles().y
			);
			yield return subitrWalkToward.Current;
		}

		//Quaternion qPlayerStart = playerController.transform.rotation;
		//Quaternion qPlayerEnd = transform.rotation;
		//playerController.transform.rotation = qPlayerEnd;
		subitrTurnPlayer.reset(playerController.transform.rotation,transform.rotation);
		yield return new ParallelEnumerator(this,
			playerController.rfUnlock(tKeyHole.position),
			subitrTurnPlayer
		);
		
		//yield return KeyManager.Instance.tweenIconKeyPick(transform.position,false);
		animPlayerUnlock.play(clipUnlock);
		SfxPlayer.Instance.play(sfxpfUnlock);
		yield return new WaitForSeconds(durationWaitUnlocked);
		
		//Vector3 vDoorStartPos = transform.position;
		//Collider cDoorNone = SceneMainManager.Instance
		//	.spawnDoorNone(vDoorStartPos,transform.rotation).GetComponent<Collider>();
		Collider cDoorNone = SceneMainManager.Instance.unlock(transform).GetComponent<Collider>();
		HeadLookController.Instance.setHeadLookTarget(null);
		cDoorNone.enabled = false; //halt OnTriggerEnter until lifting finishes
		SfxPlayer.Instance.play(sfxpfSlide);
		subitrDoorLift.reset(
			transform.position,
			transform.position + new Vector3(0.0f,distanceLift,0.0f)
		);
		cBlocker.enabled = false; //prevent Camera jumps when cBlocker lifts off its raycast
		yield return subitrDoorLift;
		/* This is a hotfix to prevent failure when giving up while door is lifting.
		More systematic way is to rearrange LockedDoor GameObject hierarchy so that
		door mesh becomes a child of trigger which will not lift up. This is too large
		change that may risk breaking other parts, so we do this simple check for now. */
		if(PlayerController.Instance.InputMode == eInputMode.Freeze){
			yield break;}
		cBlocker.enabled = true;
		cDoorNone.enabled = true;
		subitrPanCamera.reset(camInfoStart,subitrPanCamera.End);
		yield return rfEndInspectSequence();
		SceneMainManager.Instance.CvTogglerBalloon.setActiveCanvas(false);
		gameObject.SetActive(false);
	}
	protected override void OnDisable(){
		base.OnDisable();
		animPlayerUnlock?.stopLayer();
	}
}
