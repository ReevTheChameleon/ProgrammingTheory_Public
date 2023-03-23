using UnityEngine;
using Chameleon;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class BgmPlayer : LoneMonoBehaviour<BgmPlayer>{
	AudioSource aus;
	private LoneCoroutine routineLerpVolume = new LoneCoroutine();

	public AudioClip ClipCurrent{ get{return aus.clip;} }
	protected override void Awake(){
		base.Awake();
		aus = GetComponent<AudioSource>();
		aus.loop = true;
	}
	public void playBgm(AudioClip clipBgm,float volume=1.0f){
		aus.clip = clipBgm;
		aus.volume = volume;
		aus.Play();
	}
	public void playBgm(AudioPrefab apfBgm){
		playBgm(apfBgm.audioClip,apfBgm.volume);
	}
	public WaitLoneCoroutine lerpVolume(float volumeTarget,float duration){
		return routineLerpVolume.start(
			this,
			aus.tweenVolume(aus.volume,volumeTarget,duration)
		);
	}
}
