using UnityEngine;
using UnityEngine.InputSystem;
using Chameleon;
using System.Collections;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class CursorController{
	public static bool ShowCursor{
		get{return Cursor.lockState != CursorLockMode.Locked;}
		set{Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;}
	}
}

public enum eInputMode{ MainGameplay,Interacting,Freeze }

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(AnimationPlayer))]
public class PlayerController : LoneMonoBehaviour<PlayerController>{
	PlayerInput playerInput;
	AnimationPlayer animPlayer;
	Rigidbody rb;
	
	[Header("InputMove")]
	[SerializeField] InputActionID actionIDMove;
	[SerializeField] AnimationClip clipIdle;
	[SerializeField] AnimationClip clipRun;
	[SerializeField] float transitionTime = 0.2f;
	[SerializeField] float turnSpeed;
	[SerializeField] AudioPrefab sfxpfFootstepRun;
	[SerializeField] float weightFootstepRunTrigger;
	private Vector2 v2Direction;
	private LoneCoroutine routineMove;
	private const float RUNSPEED = 2.939501f/0.7f;
	private bool bMoving;
	private LoneCoroutine routineTurn = new LoneCoroutine();
	private float targetFacingAngle;
	private float facingAngle;
	//private Vector3 vMoveDirection;

	[Header("InputLook")]
	[SerializeField] InputActionID actionIDLook;
	[SerializeField] Transform tVCamTarget;
	[SerializeField] Vector2 v2LookRangeVertical;
	[SerializeField] float inputLookSensitivity;
	[Bakable] public static float inputLookMultiplier = 0.7f; 

	[Header("InputZoom")]
	[SerializeField] InputActionID actionIDZoom;
	[SerializeField] ThirdPersonCameraControl cameraControl;
	[SerializeField] Vector2 rangeZoom;
	[SerializeField] float inputZoomSensitivity;

	[Header("InputPause")]
	[SerializeField] InputActionID actionIDPause;

	[Header("InputInteract")]
	[SerializeField] InputActionID actionIDInteract;
	[SerializeField] AnimationClip clipLeftTurn90;
	[SerializeField] AnimationClip clipRightTurn90; //Unity cannot mirror generic rig, so need this
	[SerializeField] Vector2 rangeTurnWeight;
	[SerializeField] float turnTime;
	[SerializeField] AnimationClip clipPickup;
	[SerializeField] float clipPickupSpeed;
	[SerializeField] float clipPickupTransitionTime;
	[SerializeField] AudioPrefab sfxpfUseKey;

	[SerializeField] float distancePickup;
	[SerializeField] Vector3 vOpenOffset;

	[SerializeField] AnimationClip clipOpen;
	[SerializeField] Vector2 openTransitionTime;

	[Header("InputUse")]
	[SerializeField] InputActionID actionIDUse;

	public enum eCutsceneMotion{Walk=0,WalkBackward,SlideLeft,SlideRight}
	[Header("Walk")]
	[Tooltip("Walk=0,WalkBackward,SlideLeft,SlideRight")]
	[SerializeField] public AnimationTree treeCutsceneMotion;
	/* Element's value can still be changed. C# don't have actual const array (Credit: Roger Hill, SO)
	So make it private and don't change it in the class. */
	[SerializeField][FixedSize(typeof(eCutsceneMotion))] float[] aSpeed;
	[SerializeField] Vector2 rangeWalkWeight;
	[SerializeField] float walkTransitionTime;
	[SerializeField] AudioPrefab sfxpfFootstepWalk;
	[SerializeField] float weightFootstepWalkTrigger;

	[Header("Damage")]
	[SerializeField] AnimationClip clipDamage;
	[SerializeField] float clipDamageSpeed;
	[SerializeField] float clipWeightMultiplier;
	[SerializeField] Vector2 rangeWeightDamage;
	//[SerializeField] AvatarMask maskDamage;
	[SerializeField] List<Material> lSharedMatPlayer;
	[SerializeField] Color[] aColorPlayer;
	[SerializeField] Color tintDamage;
	[SerializeField] float tintLerp;
	[SerializeField] float durationTintDamage;
	[SerializeField] AudioPrefab sfxpfDamage;
	
	[Header("Die")]
	[SerializeField] AnimationClip clipDie;
	[SerializeField] float durationTransitionDie;

	public float DistancePickup{ get{return distancePickup;} }
	public Vector3 VOpenOffset{ get{return vOpenOffset;} }
	private bool bPause = false;
	public bool IsPause{
		get{ return bPause; }
		set{
			if(value){ //pausing
				playerInput.actions[actionIDMove].Disable();
				playerInput.actions[actionIDLook].Disable();
				playerInput.actions[actionIDZoom].Disable();
				playerInput.actions[actionIDInteract].Disable();
				playerInput.actions[actionIDUse].Disable();
				playerInput.actions[actionIDPause].Enable();
			}
			else{ //unpausing
				applyInputMode();}
			bPause = value;
		}
	}
	private eInputMode inputMode = eInputMode.MainGameplay;
	public eInputMode InputMode{
		get{ return inputMode; }
		set{
			if(inputMode != value){
				inputMode = value;
				routineTurn.stop();
				routineMove.stop();
				animPlayer.transitionTo(clipIdle,transitionTime);
				if(!bPause){
					applyInputMode();}
			}
		}
	}
	private void applyInputMode(){
		switch(inputMode){
			case eInputMode.MainGameplay:
				playerInput.actions[actionIDMove].Enable();
				playerInput.actions[actionIDLook].Enable();
				playerInput.actions[actionIDZoom].Enable();
				playerInput.actions[actionIDInteract].Enable();
				playerInput.actions[actionIDUse].Enable();
				playerInput.actions[actionIDPause].Enable();
				//ShowCursor = false;
				break;
			case eInputMode.Interacting:
				playerInput.actions[actionIDMove].Disable();
				playerInput.actions[actionIDLook].Disable();
				playerInput.actions[actionIDZoom].Disable();
				playerInput.actions[actionIDInteract].Enable();
				playerInput.actions[actionIDUse].Disable();
				playerInput.actions[actionIDPause].Enable();
				//ShowCursor = true;
				break;
			case eInputMode.Freeze:
				playerInput.actions[actionIDMove].Disable();
				playerInput.actions[actionIDLook].Disable();
				playerInput.actions[actionIDZoom].Disable();
				playerInput.actions[actionIDInteract].Disable();
				playerInput.actions[actionIDUse].Disable();
				playerInput.actions[actionIDPause].Disable();
				//ShowCursor = false;
				break;
		}
	}

	protected override void Awake(){
		base.Awake();
		playerInput = GetComponent<PlayerInput>();
		rb = GetComponent<Rigidbody>();
		Renderer[] aRenderer = GetComponentsInChildren<Renderer>();
		for(int i=0; i<aRenderer.Length; ++i){
			lSharedMatPlayer.AddRange(aRenderer[i].sharedMaterials);}
		aColorPlayer = new Color[lSharedMatPlayer.Count];
		for(int i=0; i<lSharedMatPlayer.Count; ++i){
			aColorPlayer[i] = lSharedMatPlayer[i].color;}

		animPlayer = GetComponent<AnimationPlayer>();
		animPlayer.addLayer(1,null,true);
		animPlayer.addLayer(2,null,true);
		animPlayer.getPlayableController(clipPickup).addAnimationActionAtEvent(
			0,()=>{ (Interactable.Focused as PickableInspectable)?.onPicked(); }
		);
		animPlayer.getPlayableController(clipOpen).addAnimationActionAtEvent(
			0,()=>{
				KeyManager.Instance.removeKey();
				KeyManager.Instance.tweenIconKeyPick(vDoorPos,false);
				SfxPlayer.Instance.play(sfxpfUseKey);
			}
		);
		animPlayer.getPlayableController(clipDamage,2).addEndAnimationAction(
			()=>{animPlayer.stopLayer(2);}
		);
		
		ClipPlayableController controllerRun = animPlayer.getPlayableController(clipRun);
		Action dRunFootstep =
			()=>{playFootstepSound(controllerRun,sfxpfFootstepRun,weightFootstepRunTrigger);};
		controllerRun.addAnimationActionAtEvent(0,dRunFootstep);
		controllerRun.addAnimationActionAtEvent(1,dRunFootstep);

		TreePlayableController controllerTreeWalk =
			animPlayer.getPlayableController(treeCutsceneMotion);
		for(int i=0; i<controllerTreeWalk.SlotCount; ++i){
			ClipPlayableController controllerWalk = (ClipPlayableController)controllerTreeWalk[i];
			for(int j=0; j<controllerWalk.Clip.events.Length; ++j){
				controllerWalk.addAnimationActionAtEvent(
					j,
					()=>{playFootstepSound(controllerWalk,sfxpfFootstepWalk,weightFootstepWalkTrigger);}
				);
			}
		}

		routineMove = new LoneCoroutine(this,rfMove());
		InputMode = eInputMode.Freeze;
		CursorController.ShowCursor = false;
	}
	void OnEnable(){
		playerInput.actions[actionIDMove].performed += onInputMove;
		playerInput.actions[actionIDMove].canceled += onInputIdle;
		playerInput.actions[actionIDLook].performed += onInputLook;
		playerInput.actions[actionIDZoom].performed += onInputZoom;
		playerInput.actions[actionIDInteract].performed += onInputInteract;
		playerInput.actions[actionIDPause].performed += onInputEsc;
		playerInput.actions[actionIDUse].performed += onInputUse;
	}
	void OnDisable(){
		playerInput.actions[actionIDMove].performed -= onInputMove;
		playerInput.actions[actionIDMove].canceled -= onInputIdle;
		playerInput.actions[actionIDLook].performed -= onInputLook;
		playerInput.actions[actionIDZoom].performed -= onInputZoom;
		playerInput.actions[actionIDInteract].performed += onInputInteract;
		playerInput.actions[actionIDPause].performed -= onInputEsc;
		playerInput.actions[actionIDUse].performed -= onInputUse;
		routineTurn.stop();
		restorePlayerColor();
	}
	void Start(){
		animPlayer.play(clipIdle);
		#if UNITY_EDITOR
		GraphVisualizerClient.Show(animPlayer.getGraph());
		#endif
	}
	private void onInputMove(InputAction.CallbackContext context){
		v2Direction = context.ReadValue<Vector2>();
		targetFacingAngle = v2Direction.forwardAngle(); //polarAngle() -90.0f; //delta from tVCamTarget forward
		routineTurn.start(this,rfTurn(targetFacingAngle));
		/* Below will cause position to be updated next frame and avoid frame skip hiccup
		(This is related to the fact that Input is processed before FrameratePhysics update
		in that frame, combine with the fact that Coroutine won't yield again in the frame
		it was started. */
		this.delayCallEndOfFrame(()=>{
			if(bMoving){
				routineMove.resume();}
		});
		animPlayer.transitionTo(clipRun,transitionTime);
		bMoving = true;
	}
	private void onInputIdle(InputAction.CallbackContext context){
		routineTurn.stop();
		routineMove.stop();
		animPlayer.transitionTo(clipIdle,transitionTime);
		bMoving = false;
	}
	private void onInputLook(InputAction.CallbackContext context){
		Vector2 v2Delta = inputLookSensitivity*inputLookMultiplier*context.ReadValue<Vector2>();
		Vector3 eulerAngles = tVCamTarget.eulerAngles;
		eulerAngles.y = (eulerAngles.y+v2Delta.x) % 360.0f;
		eulerAngles.x = Mathf.Clamp(
			tVCamTarget.getPitchAngle()-v2Delta.y,
			v2LookRangeVertical.x,
			v2LookRangeVertical.y
		);
		tVCamTarget.eulerAngles = eulerAngles;
		if(bMoving){
			//faceDirection(v2Direction.polarAngle()*Mathf.Rad2Deg -90.0f);
			transform.setEulerY(tVCamTarget.eulerAngles.y+facingAngle);
		}
	}
	//private void faceDirection(float angle){ //delta angle from tVCamTarget forward
	//	/* Trick from Unity tutorial */
	//	transform.setEulerY(tVCamTarget.eulerAngles.y-angle);
	//	//tVCamTarget.setLocalEulerY(angle);
	//	//vMoveDirection = transform.rotation*Vector3.forward;
	//}
	private void onInputZoom(InputAction.CallbackContext context){
		float delta = inputZoomSensitivity*context.ReadValue<float>(); //usually Windows is +/-120
		cameraControl.targetCameraDistance =
			Mathf.Clamp(
				cameraControl.targetCameraDistance + delta,
				rangeZoom.x,
				rangeZoom.y
			)
		;
	}
	private IEnumerator rfTurn(float angleEnd){
		float angleStart = transform.eulerAngles.y-tVCamTarget.eulerAngles.y;
		float t = 0.0f;
		float turnTime = Mathf.Max(
			0.1f,
			Mathf.Abs(Mathf.DeltaAngle(angleStart,angleEnd))/turnSpeed
		);
		//WaitForFixedUpdate wait = new WaitForFixedUpdate();
		while(t < 1.0f){
			yield return null;
			facingAngle = Mathf.LerpAngle(angleStart,angleEnd,t);
			transform.setEulerY(tVCamTarget.eulerAngles.y+facingAngle);
			t += Time.deltaTime/turnTime;
		}
		facingAngle = angleEnd;
		transform.setEulerY(tVCamTarget.eulerAngles.y+facingAngle);
	}
	private IEnumerator rfMove(){
		//WaitForFixedUpdate wait = new WaitForFixedUpdate();
		while(true){
			rb.position += RUNSPEED*Time.deltaTime*
				(Quaternion.Euler(0.0f,targetFacingAngle,0.0f)*
					tVCamTarget.forward.newY(0.0f).normalized);
			//Debug.Log((RUNSPEED*Time.fixedDeltaTime*
			//	(Quaternion.Euler(0.0f,targetFacingAngle,0.0f)*
			//		tVCamTarget.forward.newY(0.0f).normalized)).toPreciseString());
			yield return null;
		}
	}
	private void onInputInteract(InputAction.CallbackContext context){
		Interactable.Focused?.onInteracted();
	}
	public WaitLoneCoroutine turnToward(Transform tTarget){
		return routineTurn.start(this,rfTurnToward(tTarget.position-transform.position));
	}
	public WaitLoneCoroutine turnToward(Vector3 vDirection){
		return routineTurn.start(this,rfTurnToward(vDirection));
	}
	private IEnumerator rfTurnToward(Vector3 vDirection){
		float eulerYStart = transform.eulerAngles.y;
		float eulerYEnd = -vDirection.xz().polarAngle() + 90.0f;
		//Because eulerAngles is positive clockwise
		
		//animPlayer.addLayer(1,null,true);
		float deltaAngle = Mathf.DeltaAngle(eulerYStart,eulerYEnd);
		PlayableController playableController= animPlayer.play(
			deltaAngle>=0 ? clipLeftTurn90 : clipRightTurn90,
			1
		);
		animPlayer.setLayerWeight(1,Mathf.Clamp(
			Mathf.Abs(Mathf.DeltaAngle(eulerYStart,eulerYEnd)/90.0f),
			rangeTurnWeight.x,
			rangeTurnWeight.y
		));
		
		float t = 0.0f;
		while(t < 1.0f){
			yield return null;
			Quaternion qLookTarget = tVCamTarget.rotation;
			facingAngle = Mathf.LerpAngle(eulerYStart,eulerYEnd,t);
			transform.setEulerY(facingAngle);
			//tVCamTarget.rotation = qLookTarget; //restore camera rotation
			t += Time.deltaTime/turnTime;
		}
		animPlayer.stopLayer(1);
	}

	public IEnumerator rfWalkToward(Vector3 vPosition,eCutsceneMotion motion,bool bIgnoreY=false){
		Vector3 vPosStart = transform.position;
		float distance = Vector3.Distance(vPosition,vPosStart);
		float animationWeight = MathfExtension.clamp(distance,rangeWalkWeight);
		float walkSpeed = 0.0f;
		for(int i=0; i<treeCutsceneMotion.lTree?.Count; ++i){
			if(i == (int)motion){
				treeCutsceneMotion[i].weight = animationWeight;
				walkSpeed = aSpeed[i]*0.9f;
				/* Assuming walk animation has const speed, there should be a more correct
				kinematic formula that better sync animation and movement, but this suffices for now.
				(The reason we don't use root motion is that with it we can't walk to
				final position accurately because we don't know WHEN to transition back to idle.) */
			}
			else
				treeCutsceneMotion[i].weight = 0.0f;
		}
		animPlayer.transitionTo(treeCutsceneMotion,walkTransitionTime,0);
		float totalTime = distance/walkSpeed/animationWeight;
		float time = 0.0f;
		Coroutine c = this.delayCall(
			() => {animPlayer.transitionTo(clipIdle,walkTransitionTime,0);},
			totalTime-walkTransitionTime
		);
		float yStart = 0.0f;
		while(time < totalTime){
			yield return null;
			yStart = rb.position.y;
			rb.position = Vector3.Lerp(vPosStart,vPosition,Mathf.SmoothStep(0.0f,1.0f,time/totalTime));
			if(bIgnoreY){
				rb.position = rb.position.newY(yStart);}
			time += Time.deltaTime; //perhaps should be Time.fixedDeltaTime; will study later
		}
		rb.position = bIgnoreY ? vPosition.newY(yStart) : vPosition;
		StopCoroutine(c); //will think of more elegant way later.
	}
	private void onInputEsc(InputAction.CallbackContext context){
		//#if UNITY_WEBGL
		//ShowCursor = false;
		//#endif
		SceneMainManager.Instance.togglePause();
	}

	public IEnumerator rfPickup(){
		PlayableController controller = animPlayer.transitionTo(clipPickup,clipPickupTransitionTime);
		controller.setSpeed(clipPickupSpeed);
		yield return new WaitForSeconds(clipPickup.length/clipPickupSpeed);
		animPlayer.transitionTo(clipIdle,clipPickupTransitionTime);
		yield return new WaitForSeconds(clipPickupTransitionTime);
	}
	//void onAnimEventPickup(){
	//	(Interactable.Focused as PickableInspectable)?.onPicked();
	//}

	private Vector3 vDoorPos;
	public IEnumerator rfUnlock(Vector3 vDoorPos){
		this.vDoorPos = vDoorPos;
		animPlayer.transitionTo(clipOpen,openTransitionTime.x);
		yield return new WaitForSeconds(clipOpen.length);
		animPlayer.transitionTo(clipIdle,openTransitionTime.y);
		yield return new WaitForSeconds(openTransitionTime.y);
	}
	//public void onAnimEventLockTouch(){
	//	KeyManager.Instance.removeKey();
	//	KeyManager.Instance.tweenIconKeyPick(vDoorPos,false);
	//}
	public void damagePlayer(float amount){ //full=1.0f
		HpBarController.Instance.addHp(-amount);
		animPlayer.play(clipDamage,2).setSpeed(clipDamageSpeed);
		animPlayer.setLayerWeight(
			2,MathfExtension.clamp(amount*clipWeightMultiplier,rangeWeightDamage));
		SfxPlayer.Instance.play(sfxpfDamage);
		for(int i=0; i<lSharedMatPlayer.Count; ++i){
			lSharedMatPlayer[i].color =
				Color.Lerp(lSharedMatPlayer[i].color,tintDamage,tintLerp);
		}
		this.delayCall(restorePlayerColor,durationTintDamage);
		/* This may cause quick consecutive damage to not show,
		but it's OK for scope of this game. (Fix = use LoneCoroutine)*/
	}
	private void restorePlayerColor(){
		for(int i=0; i<lSharedMatPlayer.Count; ++i){
			lSharedMatPlayer[i].color = aColorPlayer[i];}
	}
	//void onAnimEventDamageEnd(){
	//	animPlayer.stopLayer(2);
	//}
	public void healPlayer(float amount){
		HpBarController.Instance.addHp(amount);
	}

	private void onInputUse(InputAction.CallbackContext context){
		CandleManager.Instance.toggleCandleLight();
	}
	public IEnumerator rfDie(){
		yield return animPlayer.transitionTo(clipDie,durationTransitionDie).WaitEndAnimation;
		animPlayer.resetBinding();
		animPlayer.stopLayer(0);
	}

	public void finalTouch(){
		ClipPlayableController controllerOpen = animPlayer.getPlayableController(clipOpen);
		controllerOpen.removeAnimationAction(0);
		controllerOpen.addAnimationActionAtEvent(
			0,() => {
				Debug.Log("StopLayer 0");
				animPlayer.resetBinding();
				animPlayer.stopLayer(0);
				Debug.Log("StopLayer 0 done");
				//animPlayer.stopLayer(0);
			}
		);
		animPlayer.transitionTo(clipOpen,transitionTime);
		//animPlayer.stopLayer(0);
	}

	private void playFootstepSound(
		PlayableController playableController,AudioPrefab sfxpf,float weightTrigger)
	{
		if(playableController.getSelfWeight() > weightTrigger){
			SfxPlayer.Instance.play(sfxpf);}
	}

	#if ENABLE_PROFILER	
	void Update(){
	//	if(Keyboard.current.xKey.wasPressedThisFrame)
	//		turnToward(tTest);
	//	if(Keyboard.current.yKey.wasPressedThisFrame)
	//		animPlayer.play(clipRightTurn90);
	//	if(Keyboard.current.cKey.wasPressedThisFrame)
	//		ShowCursor = true;
	//	if(Keyboard.current.hKey.wasPressedThisFrame)
	//		ShowCursor = false;
	//	if(Keyboard.current.lKey.wasPressedThisFrame){
	//		//PlayableController p = animPlayer.play(clipWalk);
	//		//p.setSpeed(-1.0);
	//		StartCoroutine(rfWalkToward(tTest.position.newY(0.0f)));
	//		//PlayableController p = animPlayer.transitionTo(clipWalk,transitionTime);
	//		//p.setSpeed(-1.0);
	//	}
	//	if(Keyboard.current.kKey.wasPressedThisFrame){
	//		//treeWalk[0].weight = 0.1f;
	//		animPlayer.play(treeWalkBackward,0);
	//		bWalk = true;
	//	}
	//	if(Keyboard.current.kKey.wasReleasedThisFrame){
	//		animPlayer.play(clipIdle);
	//		bWalk = false;
	//	}
		if(Keyboard.current.jKey.wasPressedThisFrame)
			animPlayer.play(treeCutsceneMotion[3].clip);
		if(Keyboard.current.rKey.wasPressedThisFrame)
			StartCoroutine(rfUnlock(transform.position));
		if(Keyboard.current.uKey.wasPressedThisFrame){
			animPlayer.play(clipDamage,2);
		}
		if(Keyboard.current.upArrowKey.wasPressedThisFrame){
			animPlayer.setLayerWeight(2,0.5f);
		}
		if(Keyboard.current.upArrowKey.wasPressedThisFrame){
			animPlayer.setLayerWeight(2,2.0f);
			CandleManager.Instance.addLight(0.5f);
		}
		if(Keyboard.current.downArrowKey.wasPressedThisFrame){
			HpBarController.Instance.addHp(-0.2f); }
		if(Keyboard.current.numpad0Key.wasPressedThisFrame){
			animPlayer.transitionTo(clipDie,durationTransitionDie);
		}
		if(Keyboard.current.numpad1Key.wasPressedThisFrame){
			SceneMainManager.Instance.onDie();
		}
	}
	#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerController))]
class PlayerControllerEditor : MonoBehaviourBakerEditor{ }
#endif
