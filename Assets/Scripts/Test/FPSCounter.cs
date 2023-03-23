using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Chameleon{

/* Using this instead of [RemoveOnBuild] so you can keep it in development build */
[ExecuteOnBuild(nameof(removeFromBuild),bApplyInEditorBuild=true)]
public class FPSCounter : LoneMonoBehaviour<FPSCounter>{
	[SerializeField] int fontSize; //0 will use default
	[SerializeField] Vector2 v2ScreenPos;
	[SerializeField] Color colorText = Color.white;
	
	private string sFps;
	private Rect rectLabel = new Rect(0,0,0,0);
	private GUIStyle styleText;
	private static bool bShowFPS = true;
	/* I use bShowFPS as staic (global) for all FPSCounter because you can toggle each one
	on/off easily, but the most problem is you are not sure whether you have turned them
	ALL off or not during release build. */

	protected override void Awake(){
		base.Awake();
		updateSettings();
		useGUILayout = false;
		/* Avoid memory allocation related to GUILayout, which isn't used anyway (Credit: Bunny83, UF) */
	}
	/* Choose to use IMGUI to avoid complication with Canvases and position,
	since the text is updated every frame anyway.*/
	void OnGUI(){
		GUI.Label(
			rectLabel,
			sFps,
			styleText
		);
	}
	void Update(){
		/* Create string here because OnGUI() may be called more than once per frame, and
		each time the string is created, it allocates some memory. Still, allocation
		for string is unavoidable because C# string is immutable, unless you hack around
		with unsafe code (Credit: Bunny83, UA). Here, we try to reduce it as much as possible.
		Note that this still happens even when you use normal UI rather than IMGUI. */
		//Make FPS only 2 decimal places (Credit: WraithNath & Michael, SO)
		sFps = "FPS: "+(1/Time.deltaTime).ToString("0.00") + " ("+Time.deltaTime*1000.0f+" ms)";
	}
	private void updateSettings(){
		rectLabel = new Rect(v2ScreenPos.x,v2ScreenPos.y,0,0);
		styleText = new GUIStyle();
		styleText.normal.textColor = colorText;
		styleText.fontSize = fontSize;
	}
	private void removeFromBuild(){
		if(!bShowFPS){
			DestroyImmediate(gameObject);}
	}

	#if UNITY_EDITOR
	private const string PlayerPrefsKeyShowFPS = "chm_FPSCounter_ShowFPS";

	protected override void OnValidate(){
		base.OnValidate();
		updateSettings();
	}
	[InitializeOnLoadMethod]
	static void loadShowFPSPrefs(){
		bShowFPS = PlayerPrefs.HasKey(PlayerPrefsKeyShowFPS);
	}
	static void setShowFPSPrefs(bool bShow){
		if(bShow){
			PlayerPrefs.SetInt(PlayerPrefsKeyShowFPS,1);}
		else{ //not show
			PlayerPrefs.DeleteKey(PlayerPrefsKeyShowFPS);}
	}
	
	[CustomEditor(typeof(FPSCounter))]
	class FPSCounterEditor : Editor<FPSCounter>{
		public override void OnInspectorGUI(){
			DrawDefaultInspector();
			EditorGUI.BeginChangeCheck();
			FPSCounter.bShowFPS =
				EditorGUILayout.Toggle("[Global] Show in Build",FPSCounter.bShowFPS);
			if(EditorGUI.EndChangeCheck()){
				FPSCounter.setShowFPSPrefs(bShowFPS);}
		}
	}
	#endif
}

} //end namespace Chameleon
