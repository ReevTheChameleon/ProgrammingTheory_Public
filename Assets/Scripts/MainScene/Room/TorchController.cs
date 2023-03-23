using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TorchController : MonoBehaviour{
	[SerializeField] GameObject[] agFire;

	public void setLight(bool bLit){
		for(int i=0; i<agFire.Length; ++i){
			agFire[i].SetActive(bLit); }
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(TorchController))]
	class TorchControllerEditor : Editor{
		private TorchController targetAs;
		void OnEnable(){
			targetAs = (TorchController)target;
		}
		public override void OnInspectorGUI(){
			DrawDefaultInspector();
			if(GUILayout.Button("Light on")){
				targetAs.setLight(true); }
			if(GUILayout.Button("Light off")){
				targetAs.setLight(false); }
		}
	}
	#endif
}
