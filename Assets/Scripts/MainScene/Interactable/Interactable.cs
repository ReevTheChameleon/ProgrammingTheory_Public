using UnityEngine;
using Chameleon;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Profiling;
#endif

/* Note: Currently the code will not work well if 2 triggers overlap.
It is possible to fix, but if there is no overlap triggers, it wastes performance,
so it is not implemented. */
[RequireComponent(typeof(Collider))]
public abstract class Interactable : MonoBehaviour{
	[Bakable][Tag] protected const string tagPlayer = "Player";
	[SerializeField] protected Vector2 rangeEulerYInteractable;
	[SerializeField] protected Vector2 rangeEulerYLook;
	[SerializeField][ShowPosition(true,"Balloon")] protected Vector3 vBalloonPos;
	[SerializeField] protected Transform tLookTarget;
	protected Transform tPlayer;
	protected bool bHideBalloon;
	private bool bActivatingBalloon = false;
	protected bool bLurking; //if true, will only appear if lit

	public static Interactable Focused{get; private set;} = null;
	public static bool interact(){
		if(Focused){
			Focused.onInteracted();
			return true;
		}
		return false;
	}
	protected virtual void Awake(){
		enabled = false;
	}
	protected virtual void OnEnable(){
		bLurking = SceneMainManager.Instance.IsCurrentRoomDark;
	}
	protected virtual void OnDisable(){
		if(Focused == this){
			deactivate();}
	}
	protected virtual void OnTriggerEnter(Collider other){
		//if(SceneMainManager.Instance.IsCurrentRoomDark && !CandleManager.Instance.IsLit){
		//	return; }
		if(other.CompareTag(tagPlayer)){
			this.enabled = true;
			//activate();
			Update();
		}
	}
	protected virtual void OnTriggerExit(Collider other){
		//if(SceneMainManager.Instance.IsCurrentRoomDark && !CandleManager.Instance.IsLit){
		//	return; }
		if(other.CompareTag(tagPlayer)){
			deactivate();
			this.enabled = false;
		}
	}
	protected virtual void activate(){
		Focused = this;
		CanvasToggler cvTogglerBalloon = SceneMainManager.Instance.CvTogglerBalloon;
		if(!cvTogglerBalloon)
			return; //prevent MissingReferenceException if called from OnDisable() on scene unloaded
		cvTogglerBalloon.transform.position = transform.TransformPoint(vBalloonPos);
		cvTogglerBalloon.setActiveCanvas(true);
		//HeadLookController.Instance.setHeadLookTarget(tLookTarget);
		bActivatingBalloon = true;
	}
	protected virtual void deactivate(){
		Focused = null;
		CanvasToggler togglerBalloon =
			SceneMainManager.Instance ? SceneMainManager.Instance.CvTogglerBalloon : null;
		if(!togglerBalloon)
			return; //prevent MissingReferenceException if called from OnDisable() on scene unloaded
		togglerBalloon.setActiveCanvas(false);
		HeadLookController.Instance.setHeadLookTarget(null);
		bActivatingBalloon = false;
	}
	protected virtual void Update(){
		if(bLurking && !CandleManager.Instance.IsLit){
			deactivate();
			return;
		}
		if(bHideBalloon)
			return;
		UnityEngine.Profiling.Profiler.BeginSample("UpdateInteractable");
		bool bInRange = isInYLookRange(rangeEulerYInteractable);
		if(bInRange){
			if(!bActivatingBalloon){
				activate(); }
			HeadLookController.Instance.setHeadLookTarget(
				isInYLookRange(rangeEulerYLook) ?
				tLookTarget :
				null
			);
		}
		if(!bActivatingBalloon && bInRange)
			activate();
		else if(bActivatingBalloon && !bInRange)
			deactivate();
		UnityEngine.Profiling.Profiler.EndSample();
	}
	protected bool isInYLookRange(Vector2 rangeAngle){
		Transform tPlayer = PlayerController.Instance.transform;
		float deltaAngle = Mathf.DeltaAngle(
			tPlayer.eulerAngles.y,
			(transform.position-tPlayer.position).eulerAngles().y
		);
		return 
			deltaAngle>=rangeAngle.x && 
			deltaAngle<=rangeAngle.y
		;
	}
	public abstract void onInteracted();
}

#if UNITY_EDITOR
[CustomEditor(typeof(Interactable),true)]
class InteractableEditor: MonoBehaviourBakerEditorWithScene{}
#endif
