#if UNITY_EDITOR

using UnityEngine;
using Chameleon;

[RemoveOnBuild]
[ExecuteInEditMode]
public class FramerateLimiter : MonoBehaviour{
	[Min(1)] public int targetFrameRate = 60;

	void OnValidate(){
		if(enabled){
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = targetFrameRate;
		}
	}
	void OnDisable(){
		QualitySettings.vSyncCount = 1;
		Application.targetFrameRate = -1;
	}
}

#endif
