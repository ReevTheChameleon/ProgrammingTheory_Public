using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Chameleon{

public class Slideshow : MonoBehaviour{
	public enum eSlideDirection{Left,Right,Up,Down}
	public enum eGraphicType{Image,RawImage}

	[SerializeField][GrayOnPlay] eGraphicType graphicType;
	[SerializeField] Mask mask;
	[SerializeField] Graphic graphic1;
	[SerializeField] Graphic graphic2;
	[SerializeField] List<Texture> lTexture;
	[SerializeField] List<Sprite> lSprite;
	[SerializeField] eSlideDirection slideDirection;
	[SerializeField] AnimationCurve curveSlidePos;
	[SerializeField] float durationSlide;
	[SerializeField] float durationStay;
	[SerializeField][GrayOnPlay] bool bSlideOnStart;
	public bool bLoop;
	public int startIndex;

	private Graphic graphicPrev;
	private Graphic graphicCurrent;
	private LoneCoroutine routineSlideshow = new LoneCoroutine();
	private LoneCoroutine routineManualSlide = new LoneCoroutine();
	private RectTransform rt;
	private Cooldown cooldown = new Cooldown();
	private TweenRoutineUnit subitrSlide;
	private int index = 0;
	private eSlideDirection activeDirection;
	private IList ilSlide;
	
	public delegate void DOnSlide(int indexTarget);
	public event DOnSlide evOnSlideStart;
	public event DOnSlide evOnSlideFinish;
	public event Action evOnSlideshowEnd;
	
	public int Length{
		get{ return ilSlide.Count; }
	}
	public int Index{
		get{return index;}
		set{
			index = value;
			setTexture(index);
		}
	}
	public bool IsSliding{get; private set;}
	public bool IsRunning{
		get{ return routineSlideshow.IsRunning; }
	}
	public float DurationSlide{
		get{ return durationSlide; }
		set{
			if(subitrSlide != null){
				subitrSlide.Reset(value);
				subitrSlide.skip();
			}
			durationSlide = value;
		}
	}
	public float DurationStay{
		get{ return durationStay; }
		set{ durationStay = value; }
	}
	public eSlideDirection SlideDirection{
		get{ return slideDirection; }
		set{
			skipSliding();
			slideDirection = value;
		}
	}
	public void setLTexture(List<Texture> lTexture){ //Maybe should make deep copy?
		#if UNITY_EDITOR
		if(graphicType != eGraphicType.RawImage){
			Debug.LogWarning("Graphic type cannot accept texture"); }
		#endif
		this.lTexture = lTexture;
		setActiveList(graphicType);
	}
	public void setLSprite(List<Sprite> lSprite){
		#if UNITY_EDITOR
		if(graphicType == eGraphicType.RawImage){
			Debug.LogWarning("Graphic type cannot accept sprite"); }
		#endif
		this.lSprite = lSprite;
		setActiveList(graphicType);
	}
	void Awake(){
		setActiveList(graphicType);
		graphicPrev = graphic1;
		graphicCurrent = graphic2;
		rt = (RectTransform)transform;
		subitrSlide = new TweenRoutineUnit(
			setSlideFraction,
			durationSlide
		);
	}
	void Start(){
		graphicPrev.rectTransform.anchoredPosition = new Vector2(-rt.rect.width,0.0f); //just move somewhere else
		Index = startIndex;
		if(bSlideOnStart){
			routineSlideshow.start(this,rfSlideshow());}
	}
	public void start(){
		routineManualSlide.stop();
		Index = startIndex;
		activeDirection = slideDirection;
		routineSlideshow.start(this,rfSlideshow());
	}
	public void stop(){
		skipSliding();
		routineSlideshow.stop();
	}
	public void pause(){
		routineSlideshow.stop();
	}
	public void resume(){
		routineManualSlide.stop();
		activeDirection = slideDirection;
		routineSlideshow.resume();
	}
	public void skipSliding(){ //jump to slide end if sliding
		if(IsSliding){
			subitrSlide.skip();
			IsSliding = false;
		}
	}
	public int jumpNext(bool bWrap=true){
		if(IsSliding){
			skipSliding();
			return index;
		}
		else if(incrementIndex(bWrap)){
			setTexture(index);}
		return index;
	}
	public int jumpPrevious(bool bWrap=true){
		if(decrementIndex(bWrap)){
			setTexture(index);}
		return index;
	}
	public bool slideNext(bool bWrap=true){
		if(IsRunning){
			cooldown.clear();}
		else{
			if(!incrementIndex(bWrap)){
				return false;}
			activeDirection = slideDirection;
			routineManualSlide.start(this,rfSlideOne());
		}
		return true;
	}
	/* This functions can only be used if slideshow is NOT running
	because slide directions oppose each other and I don't know
	how best to resolve. */
	public bool slidePrevious(bool bWrap=true){
		if(IsRunning || !decrementIndex(bWrap)){
			return false;}
		activeDirection = oppositeDirection(slideDirection);
		routineManualSlide.start(this,rfSlideOne());
		return true;
	}

	private void setTexture(int index){
		float aspectRatio = 1.0f;
		switch(graphicType){
			case eGraphicType.RawImage:
				if(lTexture==null || lTexture.Count<=0 ||!lTexture[index]){
					return;}
				RawImage rimgCurrent = (RawImage)graphicCurrent;
				rimgCurrent.texture = lTexture[index];
				aspectRatio = (float)rimgCurrent.texture.height/rimgCurrent.texture.width;
				break;
			case eGraphicType.Image:
				if(lSprite==null || lSprite.Count<=0 || !lSprite[index]){
					return;}
				Image imgCurrent = (Image)graphicCurrent;
				imgCurrent.sprite = lSprite[index];
				aspectRatio = (float)
					imgCurrent.sprite.rect.height /
					imgCurrent.sprite.rect.width
				;
				break;
		}
		skipSliding();
		graphicCurrent.rectTransform.fitParent();
		graphicCurrent.rectTransform.resizeToMatchAspectRatio(aspectRatio);
	}
	private IEnumerator rfSlideshow(){
		while(true){
			cooldown.set(durationStay);
			while(cooldown){
				yield return null;}
			if(!incrementIndex(bLoop)){
				evOnSlideshowEnd?.Invoke();
				yield break;
			}
			IEnumerator subitrSlideOne = rfSlideOne();
			while(subitrSlideOne.MoveNext()){
				yield return subitrSlideOne.Current;}
		}
	}
	private IEnumerator rfSlideOne(){
		Graphic graphicTmp = graphicPrev;
		graphicPrev = graphicCurrent;
		graphicCurrent = graphicTmp;
		setTexture(index);
		subitrSlide.Reset();

		IsSliding = true;
		evOnSlideStart?.Invoke(index);
		while(subitrSlide.MoveNext()){
			yield return subitrSlide.Current;}
		IsSliding = false;
		evOnSlideFinish?.Invoke(index);
	}
	private void setSlideFraction(float fraction){
		float t = curveSlidePos.Evaluate(fraction);
		switch(activeDirection){
			case eSlideDirection.Left:
				graphicPrev.rectTransform.anchoredPosition = new Vector2(-t*rt.rect.width,0.0f);
				graphicCurrent.rectTransform.anchoredPosition = new Vector2(-(t-1)*rt.rect.width,0.0f);
				break;
			case eSlideDirection.Right:
				graphicPrev.rectTransform.anchoredPosition = new Vector2(t*rt.rect.width,0.0f);
				graphicCurrent.rectTransform.anchoredPosition = new Vector2((t-1)*rt.rect.width,0.0f);
				break;
			case eSlideDirection.Up:
				graphicPrev.rectTransform.anchoredPosition = new Vector2(0.0f,t*rt.rect.height);
				graphicCurrent.rectTransform.anchoredPosition = new Vector2(0.0f,(t-1)*rt.rect.height);
				break;
			case eSlideDirection.Down:
				graphicPrev.rectTransform.anchoredPosition = new Vector2(0.0f,-t*rt.rect.height);
				graphicCurrent.rectTransform.anchoredPosition = new Vector2(0.0f,-(t-1)*rt.rect.height);
				break;
		}
	}
	private eSlideDirection oppositeDirection(eSlideDirection direction){
		switch(direction){
			case eSlideDirection.Down: return eSlideDirection.Up;
			case eSlideDirection.Up: return eSlideDirection.Down;
			case eSlideDirection.Left: return eSlideDirection.Right;
			case eSlideDirection.Right: return eSlideDirection.Left;
			default: return eSlideDirection.Left; //shouldn't reach here
		}
	}
	private bool incrementIndex(bool bWrap){
		if(ilSlide==null || ilSlide.Count<=0){
			return false;}
		if(++index >= ilSlide.Count){
			if(!bWrap){
				index = ilSlide.Count-1;
				return false;
			}
			index %= ilSlide.Count;
		}
		return true;
	}
	private bool decrementIndex(bool bWrap){
		if(ilSlide==null || ilSlide.Count<=0){
			return false;}
		if(--index < 0){
			if(!bWrap){
				index = 0;
				return false;
			}
			index = (index+ilSlide.Count) % ilSlide.Count;
		}
		return true;
	}
	private void setActiveList(eGraphicType graphicType){
		switch(graphicType){
			case eGraphicType.RawImage: ilSlide=lTexture; break;
			case eGraphicType.Image: ilSlide=lSprite; break;
		}
	}

	#if UNITY_EDITOR
	/* Might review later for finer control
	(e.g. so changing durationStay not resetting the slide motion) */
	void OnValidate(){
		setActiveList(graphicType);
		if(ilSlide==null || ilSlide.Count<=0 || !graphic1 || !graphic2){
			return;}
		graphicPrev = graphic1;
		graphicCurrent = graphic2;
		startIndex = Mathf.Clamp(startIndex,0,ilSlide.Count-1);
		//Because fitParent, which set anchorMin/Max can't be called here
		EditorApplication.delayCall += updateFromInspector;
	}
	private void updateFromInspector(){
		DurationSlide = durationSlide;
		DurationStay = durationStay;
		SlideDirection = slideDirection;
		if(!EditorApplication.isPlaying && this!=null){
			Awake();
			Index = startIndex;
		}
	}

	[CustomEditor(typeof(Slideshow))]
	[CanEditMultipleObjects]
	class SlideshowEditor : Editor<Slideshow>{
		private string[] aNameExcludeProperty = new string[]{"",
			nameof(mask),nameof(graphicType),nameof(graphic1),nameof(graphic2),
			"m_Script"
		};
		public override void OnInspectorGUI(){
			serializedObject.Update();
			SerializedProperty spGraphic1 = serializedObject.FindProperty(nameof(Slideshow.graphic1));
			SerializedProperty spGraphic2 = serializedObject.FindProperty(nameof(Slideshow.graphic2));
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(
				serializedObject.FindProperty(nameof(Slideshow.graphicType)));
			if(EditorGUI.EndChangeCheck()){
				spGraphic1.objectReferenceValue = null;
				spGraphic2.objectReferenceValue = null;
			}
			EditorGUILayout.PropertyField(
				serializedObject.FindProperty(nameof(Slideshow.mask)));
			Type type = typeof(UnityEngine.Object);
			switch(targetAs.graphicType){
				case eGraphicType.RawImage:
					type = typeof(RawImage);
					aNameExcludeProperty[0] = nameof(lSprite);
					break;
				case eGraphicType.Image:
					type = typeof(Image);
					aNameExcludeProperty[0] = nameof(lTexture);
					break;
			}
			EditorGUI.BeginChangeCheck();
			Graphic graphicUser1 = (Graphic)EditorGUILayout.ObjectField(
				nameof(targetAs.graphic1),
				targetAs.graphic1,
				type,
				true
			);
			Graphic graphicUser2 = (Graphic)EditorGUILayout.ObjectField(
				nameof(targetAs.graphic2),
				targetAs.graphic2,
				type,
				true
			);
			if(EditorGUI.EndChangeCheck()){
				spGraphic1.objectReferenceValue = graphicUser1;
				spGraphic2.objectReferenceValue = graphicUser2;
			}
			DrawPropertiesExcluding(serializedObject,aNameExcludeProperty);
			serializedObject.ApplyModifiedProperties();
		}
	}
	#endif

	void Update(){
		if(UnityEngine.InputSystem.Keyboard.current.numpad5Key.wasPressedThisFrame){
			start(); }
		if(UnityEngine.InputSystem.Keyboard.current.numpad6Key.wasPressedThisFrame){
			resume(); }
		if(UnityEngine.InputSystem.Keyboard.current.numpad4Key.wasPressedThisFrame){
			pause(); }
		if(UnityEngine.InputSystem.Keyboard.current.numpad8Key.wasPressedThisFrame){
			stop(); }
	}
}

} //end namespace Chameleon
