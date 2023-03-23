using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Chameleon;

public class AudioPrefab : ScriptableObject{
	public AudioClip audioClip;
	public float volume;

	#if UNITY_EDITOR
	public static AudioPrefab createAsset(AudioClip clip=null,float volume=1.0f){
		AudioPrefab audioPrefab = CreateInstance<AudioPrefab>();
		audioPrefab.audioClip = clip;
		audioPrefab.volume = volume;
		EditorHelper.createAssetAtCurrentFolder(
			audioPrefab,
			(clip?.name ?? "AudioPrefab")+".asset"
		);
		return audioPrefab;
	}

	[MenuItem("Assets/Create/Chameleon/Audio Prefab",priority =-1)]
	static void assetsCreateChameleonAudioPrefab(){
		AudioClip clip = Selection.activeObject as AudioClip;
		createAsset(clip);
	}
	#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AudioPrefab))]
class AudioInstructionEditor : Editor{
	private AudioPrefab targetAs;

	void OnEnable(){
		targetAs = (AudioPrefab)target;
	}
	public override void OnInspectorGUI(){
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AudioPrefab.audioClip)));
		EditorGUILayout.LabelField("Volume");
		Rect rectOriginal = GUILayoutUtility.GetLastRect();
		Rect rect = rectOriginal;
		rect.x += EditorGUIUtility.labelWidth;
		rect.width = rect.width-EditorGUIUtility.labelWidth-55.0f;
		float volumeUser = GUI.HorizontalSlider(rect,targetAs.volume,0.0f,1.0f);
		bool bSlider = volumeUser!=targetAs.volume;
		rect.x = rectOriginal.xMax-50.0f;
		rect.width = 50.0f;
		volumeUser = EditorGUI.FloatField(rect,volumeUser);
		if(volumeUser != targetAs.volume){
			Undo.RecordObject(targetAs,"Edit AudioPrefab Volume");
			targetAs.volume = Mathf.Max(
				0.0f,
				bSlider ? ((int)(volumeUser*1000))/1000.0f : volumeUser
				//if slider, round to 3 decimal points (like built-in EditorGUILayout.Slider)
			);
			EditorUtility.SetDirty(target);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif

public static class AudioPrefabExtension{
	public static void playOneShot(this AudioSource audioSource,
		AudioPrefab audioPrefab,float volumeMultiplier=1.0f)
	{
		audioSource.PlayOneShot(audioPrefab.audioClip,audioPrefab.volume*volumeMultiplier);
	}
}
