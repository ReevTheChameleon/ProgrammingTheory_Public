using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FollowConstraint : MonoBehaviour{
	[SerializeField] Transform tTarget;
	[SerializeField] Vector3 vOffset;

	void LateUpdate(){
		transform.position = TargetPosition;
	}
	public Vector3 TargetPosition{ get{return tTarget.position+vOffset;} }
	public void setTarget(Transform tTarget,Vector3 vOffset){
		this.tTarget = tTarget;
		this.vOffset = vOffset;
	}
	
	#if UNITY_EDITOR
	void OnValidate(){
		LateUpdate();
	}
	[CustomEditor(typeof(FollowConstraint))]
	class FollowConstraintEditor : Editor{
		private FollowConstraint targetAs;
		void OnEnable(){
			targetAs = (FollowConstraint)target;
		}
		void OnSceneGUI(){
			if(!EditorApplication.isPlaying){
				targetAs.vOffset = targetAs.transform.position - targetAs.tTarget.position; }
		}
	}
	#endif
}

