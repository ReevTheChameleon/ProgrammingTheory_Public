using UnityEngine;
using Chameleon;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneStoryManager : StoryManager{
	[TextArea][SerializeField] string[] aText;
	[SerializeField] SceneIndex indexSceneMain;
	[SerializeField] AudioPrefab apfBgm;
	[SerializeField] float durationFadeBgm;

	protected override string[] AText{
		get{return aText;}
	}
	protected override IEnumerator rfRunStory(string[] aText){
		BgmPlayer.Instance.playBgm(apfBgm);
		yield return base.rfRunStory(aText);
		yield return RoutineUnitCollection.fadeOutAudioListener(durationFadeBgm);
		advanceScene();
	}
	private void advanceScene(){
		SceneManager.LoadSceneAsync(indexSceneMain);
	}
}
