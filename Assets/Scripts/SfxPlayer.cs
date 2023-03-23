using UnityEngine;
using Chameleon;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using System;
using Object = UnityEngine.Object;

[RequireComponent(typeof(AudioSource))]
public class SfxPlayer : LoneMonoBehaviour<SfxPlayer>{
	/* PlayOneShot applies volume for each clip SEPARATELY, so there is no worry
	even when SFX with different volumes are played at the same time. */
	AudioSource aus;

	[RuntimeInitializeOnLoadMethod]
	static void onSceneLoad(){
		SceneManager.sceneLoaded += 
			(Scene scene, LoadSceneMode loadSceneMode) => {AudioListener.volume = 1.0f;};
	}
	public float VolumeSfx{
		get{ return aus.volume; }
		set{ aus.volume = value; }
	}
	protected override void Awake(){
		base.Awake();
		aus = GetComponent<AudioSource>();
	}
	public void play(AudioClip sfx,float volume=1.0f){
		if(!sfx){
			return;}
		aus.PlayOneShot(sfx,volume);
	}
	public void play(AudioData audioData){
		if(!audioData.audioClip){
			return;}
		aus.PlayOneShot(audioData.audioClip,audioData.volume);
	}
	public void play(AudioPrefab audioPrefab,float volume=1.0f){
		if(!audioPrefab){
			return;}
		aus.playOneShot(audioPrefab,volume);
	}
	#if UNITY_EDITOR
	protected override void OnValidate(){
		base.OnValidate();
		aus = GetComponent<AudioSource>();
	}
	#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SfxPlayer))]
class SfxPlayerEditor : Editor{
	private SfxPlayer targetAs;
	private Object uo;

	void OnEnable(){
		targetAs = (SfxPlayer)target;
	}
	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		EditorGUILayout.BeginHorizontal();
		uo = EditorHelper.multitypeObjectField(
			"Audio",uo,new Type[]{typeof(AudioClip),typeof(AudioPrefab)});
		bool bSavedEnable = GUI.enabled;
		GUI.enabled = EditorApplication.isPlaying;
		if(GUILayout.Button("Preview",GUILayout.Width(60.0f))){
			if(uo.GetType()==typeof(AudioClip))
				targetAs.play((AudioClip)uo);
			else if(uo.GetType()==typeof(AudioPrefab))
				targetAs.play((AudioPrefab)uo);
		}
		GUI.enabled = bSavedEnable;
		EditorGUILayout.EndHorizontal();
	}
}
#endif
