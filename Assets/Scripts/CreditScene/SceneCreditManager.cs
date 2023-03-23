using UnityEngine;
using System.Collections;
using Chameleon;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneCreditManager : LoneMonoBehaviour<SceneCreditManager>{
	[SerializeField] ButtonHandler btnHandlerReplay;
	[SerializeField] Image imgScreenCover;
	[SerializeField] float durationBlur;
	[SerializeField] float durationSuspendScene;
	[SerializeField] SceneIndex sceneIndexIntro;
	
	[Header("Shader Emission")]
	[SerializeField] Material matPortal;
	[SerializeField] ShaderPropertyID_float shaderPropEmission;
	[SerializeField] float previewEmission;
	
	private InterpolableKawaseBlurFeature renderFeatureKawaseBlur;
	private ParallelEnumerator subitrUnveil;

	protected override void Awake(){
		base.Awake();
		renderFeatureKawaseBlur =
			((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset)
				.getRendererData(0).getRendererFeature<InterpolableKawaseBlurFeature>();
		renderFeatureKawaseBlur.Pass.iteration = 0;
		renderFeatureKawaseBlur.SetActive(true);
		subitrUnveil = new ParallelEnumerator(this,
			new TweenRoutineUnit(
				(float t) => {renderFeatureKawaseBlur.Pass.iteration = Mathf.Lerp(16,0,t);},
				durationBlur
			),
			imgScreenCover.tweenAlpha(1.0f,0.0f,durationBlur,
				dOnDone:(float t) => {imgScreenCover.raycastTarget = t<=0.0f;}
			)
		);
		btnHandlerReplay.setOnClickAction(() => {StartCoroutine(rfTransitionScene());});
		matPortal.setFloat(shaderPropEmission,previewEmission);
		CursorController.ShowCursor = true;
	}
	void Start(){
		imgScreenCover.raycastTarget = true;
		StartCoroutine(subitrUnveil);
	}
	private IEnumerator rfTransitionScene(){
		btnHandlerReplay.enabled = false;
		imgScreenCover.raycastTarget = true;
		((TweenRoutineUnit)subitrUnveil[0]).bReverse = true;
		((TweenRoutineUnit)subitrUnveil[1]).bReverse = true;
		subitrUnveil.Reset();
		yield return subitrUnveil;
		yield return RoutineUnitCollection.fadeOutAudioListener(durationSuspendScene);
		SceneManager.LoadSceneAsync(sceneIndexIntro);
	}
}
