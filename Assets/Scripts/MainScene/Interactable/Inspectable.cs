using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Chameleon;

public class Inspectable : Interactable{
	[SerializeField][TextArea] protected List<string> lText = new List<string>();

	//Pan Camera Routine
	[Header("Camera Pan")]
	[SerializeField] protected float durationPan;
	[SerializeField][WideVector2] protected Vector2 rangeCamEulerX;
	[SerializeField] protected bool bLimitEulerY;
	[SerializeField][WideVector2] protected Vector2 rangeCamEulerY;
	[SerializeField] protected float targetDistanceCamera;
	[SerializeField] protected AudioPrefab sfxpfInspect;

	protected LoneCoroutine routineInteract = new LoneCoroutine();
	protected ThirdPersonCameraControl cameraControl;
	protected Transform tCamTarget;
	
	protected class CamInfo{
		public Vector3 vCamTarget;
		public Quaternion qCamTarget;
		public float camDistance;
		public CamInfo(){}
		public CamInfo(CamInfo other){
			vCamTarget = other.vCamTarget;
			qCamTarget = other.qCamTarget;
			camDistance = other.camDistance;
		}
	}
	protected TweenRoutineUnit<CamInfo> subitrPanCamera;

	protected enum eInspectionState{
		None,
		StartSequence,
		FooterTexting,
		Suspended,
		EndSequence,
	}
	protected eInspectionState inspectionState = eInspectionState.None;

	protected override void Awake(){
		base.Awake();
		cameraControl = Camera.main.GetComponent<ThirdPersonCameraControl>();
		tCamTarget = cameraControl.tTarget;
		subitrPanCamera = new TweenRoutineUnit<CamInfo>(
			(CamInfo camInfoStart,CamInfo camInfoEnd,float t) => {
				float tDamped = Mathf.SmoothStep(0.0f,1.0f,t);
				tCamTarget.position =
					Vector3.Lerp(camInfoStart.vCamTarget,camInfoEnd.vCamTarget,tDamped);
				tCamTarget.localRotation =
					Quaternion.Lerp(camInfoStart.qCamTarget,camInfoEnd.qCamTarget,tDamped);
				cameraControl.targetCameraDistance =
					Mathf.Lerp(camInfoStart.camDistance,camInfoEnd.camDistance,tDamped);
			},
			new CamInfo(),
			new CamInfo(),
			durationPan
		);
	}
	protected IEnumerator rfStartInspectSequence(){
		inspectionState = eInspectionState.StartSequence;
		PlayerController playerController = PlayerController.Instance;
		playerController.InputMode = eInputMode.Interacting;
		playerController.turnToward(transform);
		HeadLookController.Instance.setHeadLookTarget(tLookTarget);

		FollowConstraint camTargetContraint = tCamTarget.GetComponent<FollowConstraint>();
		camTargetContraint.enabled = false;
		bHideBalloon = true;
		SceneMainManager.Instance.CvTogglerBalloon.setActiveCanvas(false);

		subitrPanCamera.Start.vCamTarget = tCamTarget.position;
		subitrPanCamera.End.vCamTarget = tLookTarget.position;
		subitrPanCamera.Start.qCamTarget = tCamTarget.localRotation;
		Vector3 eulerCamTarget = tCamTarget.localEulerAngles;
		Quaternion qCamTargetEnd = Quaternion.Euler(
			MathfExtension.clamp(Mathf.DeltaAngle(0.0f,eulerCamTarget.x),rangeCamEulerX),
			bLimitEulerY ?
				MathfExtension.clampAngleDeg(
					eulerCamTarget.y,
					tLookTarget.eulerAngles.y + rangeCamEulerY.x,
					tLookTarget.eulerAngles.y + rangeCamEulerY.y
				) :
				eulerCamTarget.y
			,
			eulerCamTarget.z
		);
		subitrPanCamera.End.qCamTarget = qCamTargetEnd;
		subitrPanCamera.Start.camDistance = cameraControl.targetCameraDistance;
		subitrPanCamera.End.camDistance = targetDistanceCamera;
		subitrPanCamera.bReverse = false;
		subitrPanCamera.Reset();
		yield return subitrPanCamera;
		
		inspectionState = eInspectionState.FooterTexting;
		FooterManager footerManager = FooterManager.Instance;
		footerManager.showFooter(lText);
		while(!footerManager.IsDone)
			yield return null;

		//last passage does not wait for skip
		inspectionState = eInspectionState.Suspended;
	}
	protected IEnumerator rfEndInspectSequence(){
		inspectionState = eInspectionState.EndSequence;
		FooterManager.Instance.hideFooter();
		FollowConstraint camTargetConstraint = tCamTarget.GetComponent<FollowConstraint>();
		subitrPanCamera.Start.vCamTarget = camTargetConstraint.TargetPosition;
		if(bLimitEulerY){
			/* This keeps Camera's EulerY rather than reverting it */
			subitrPanCamera.Start.qCamTarget = Quaternion.Euler(
				subitrPanCamera.End.qCamTarget.eulerAngles.newY(
					subitrPanCamera.End.qCamTarget.eulerAngles.y));
			//subitrPanCamera.Start.qCamTarget = subitrPanCamera.Start.qCamTarget.newEulerY(
			//	subitrPanCamera.End.qCamTarget.eulerAngles.y);
		}
		subitrPanCamera.bReverse = true;
		subitrPanCamera.Reset();
		yield return subitrPanCamera;
		
		bHideBalloon = false;
		SceneMainManager.Instance.CvTogglerBalloon.setActiveCanvas(true);
		camTargetConstraint.enabled = true;
		PlayerController.Instance.InputMode = eInputMode.MainGameplay;
		inspectionState = eInspectionState.None;
	}
	public override void onInteracted(){
		switch(inspectionState){
			case eInspectionState.None:
				SfxPlayer.Instance.play(sfxpfInspect);
				routineInteract.start(this,rfStartInspectSequence());
				break;
			case eInspectionState.FooterTexting:
				FooterManager.Instance.stepFooter();
				break;
			case eInspectionState.Suspended:
				routineInteract.start(this,rfEndInspectSequence());
				break;
		}
	}
}
