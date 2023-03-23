using UnityEngine;
using TMPro;
using Chameleon;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneEndManager : StoryManager{
	[TextArea][SerializeField] string[] aTextWin;
	[TextArea][SerializeField] string[] aTextLose2; //Remove later!
	[TextArea][SerializeField] string[] aTextLose;

	[Header("Digit")]
	[SerializeField] TextMeshProUGUI txtDigit;
	[SerializeField] float durationFadeInDigit;

	[Header("Final")]
	[SerializeField] TextMeshProUGUI txtFinal;
	[SerializeField] TextMeshProUGUI txtAuthor;
	[SerializeField] ButtonHandler btnHandlerAttribution;
	[SerializeField] ButtonHandler btnHandlerReplay;
	[SerializeField] SceneIndex sceneIndexStart;
	[SerializeField] SceneIndex sceneIndexAttribution;
	[SerializeField] Image imgScreenCover;
	[SerializeField] float durationSuspendScene;

	[Header("BGM")]
	[SerializeField] AudioPrefab apfBgm;
	
	protected override string[] AText{
		get{return SceneMainManager.IsWin ? aTextWin : aTextLose;}
	}
	protected override void Awake(){
		base.Awake();
		btnHandlerAttribution.setOnClickAction(
			() => {StartCoroutine(rfTransitionScene(sceneIndexAttribution,false));});
		btnHandlerReplay.setOnClickAction(
			() => {StartCoroutine(rfTransitionScene(sceneIndexStart,true));});
	}
	protected override void Start(){
		txtDigit.text = ""+
			SceneMainManager.getDigitExit(0)+
			SceneMainManager.getDigitExit(1)+
			SceneMainManager.getDigitExit(2)
		;
		base.Start();
	}
	protected override IEnumerator rfRunStory(string[] aText){
		float alphaTxtDigit = txtDigit.alpha;
		Color32 colorEnd = txtDigit.color;
		BgmPlayer.Instance.playBgm(apfBgm);
		yield return txtDigit.tweenVerticesColor(
			new Color32(0,0,0,colorEnd.a),
			colorEnd,
			durationFadeInDigit,
			bFirstFrame: true
		);

		txtStory.gameObject.SetActive(true);
		yield return base.rfRunStory(aText);
		txtStory.gameObject.SetActive(false);
		
		txtFinal.gameObject.SetActive(true);
		txtAuthor.gameObject.SetActive(true);
		ChainEnumerator subitrTypewriteFinal = new ChainEnumerator(
			txtFinal.typewrite(typewriteSpeed),
			txtAuthor.typewrite(typewriteSpeed)
		);
		subitrTypewriteFinal.Reset();
		Cooldown cooldown = new Cooldown();
		cooldown.set(cooldownSkip);
		while(subitrTypewriteFinal.MoveNext()){
			yield return subitrTypewriteFinal.Current;
			if(triggerSkip && !cooldown){
				triggerSkip.clear();
				for(int i=0; i<subitrTypewriteFinal.Count; ++i){
					((TypewriteRoutineUnit)subitrTypewriteFinal[i]).skip();}
				break;
			}
		}
		yield return new WaitForSeconds(cooldownSkip);
		Cursor.lockState = CursorLockMode.None;
		btnHandlerAttribution.gameObject.SetActive(true);
		btnHandlerReplay.gameObject.SetActive(true);
	}
	private IEnumerator rfTransitionScene(
		SceneIndex sceneIndex,bool bSuspendScene)
	{
		//txtDigit.transform.SetParent(txtFinal.transform.root,true);
		//prevent sound/color change while switching scene
		btnHandlerAttribution.enabled = false;
		btnHandlerReplay.enabled = false;
		renderFeatureKawaseBlur.SetActive(true);
		subitrBlur.Reset();
		StartCoroutine(imgScreenCover.tweenAlpha(0.0f,1.0f,blurDuration)); //crude, but OK for now
		yield return subitrBlur;
		if(bSuspendScene){
			yield return RoutineUnitCollection.fadeOutAudioListener(durationSuspendScene); }
		SceneManager.LoadSceneAsync(sceneIndex);
	}
}
