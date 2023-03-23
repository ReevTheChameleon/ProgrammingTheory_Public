#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Chameleon;

[RemoveOnBuild]
public class LockChildSelection : MonoBehaviour{
	[SerializeField] bool bLockChild;
	void OnValidate(){
		if(bLockChild){
			for(int i=0; i<transform.childCount; ++i){
				//Credit: unity_tOwpNSr-oL7zFw, UA
				SceneVisibilityManager.instance.DisablePicking(
					transform.GetChild(i).gameObject,true); 
			}
		}
		else{
			for(int i=0; i<transform.childCount; ++i){
				//Credit: unity_tOwpNSr-oL7zFw, UA
				SceneVisibilityManager.instance.EnablePicking(
					transform.GetChild(i).gameObject,true);
			}
		}
	}
}

#endif
