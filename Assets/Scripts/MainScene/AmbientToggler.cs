using UnityEngine;
using System.Collections.Generic;
using Chameleon;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteOnBuild(nameof(refreshMaterialMap))]
#endif
public class AmbientToggler : MonoBehaviour{
	[System.Serializable]
	private struct DarkMaterialIndexMap{
		public Renderer renderer;
		public int indexDarkMaterial;
		public DarkMaterialIndexMap(Renderer r,int index){
			this.renderer = r;
			this.indexDarkMaterial = index;
		}
	}
	[Bakable][Layer] const int layerDark = 9;
	private int layerNormal;
	[SerializeField][HideInInspector] List<DarkMaterialIndexMap> lDarkMaterialMap;
	
	void Awake(){
		layerNormal = gameObject.layer;
	}
	public void setDarkAllChildren(bool bDark){
		DarkMaterialMap darkMaterialMap = DarkMaterialMap.Instance;
		for(int i=0; i<lDarkMaterialMap.Count; ++i){
			Renderer renderer = lDarkMaterialMap[i].renderer;
			int indexMaterial = lDarkMaterialMap[i].indexDarkMaterial;
			if(bDark){
				renderer.sharedMaterial =
					darkMaterialMap[lDarkMaterialMap[i].indexDarkMaterial].matDark;
				renderer.gameObject.layer = layerDark;
			}
			else{ //normal
				renderer.sharedMaterial =
					darkMaterialMap[lDarkMaterialMap[i].indexDarkMaterial].matNormal;
				renderer.gameObject.layer = layerNormal;
			}
		}
	}
	#if UNITY_EDITOR
	private void refreshMaterialMap(){
		if(BuildPipeline.isBuildingPlayer){
			return;}
		lDarkMaterialMap = new List<DarkMaterialIndexMap>();
		Renderer[] aRenderer = GetComponentsInChildren<Renderer>();
		DarkMaterialMap darkMaterialMap = DarkMaterialMap.Instance;
		for(int i=0; i<aRenderer.Length; ++i){
			Material material = aRenderer[i].sharedMaterial;
			/* This is much worse than Dictionary, but it is done during editor session and
			build-time only, so it is acceptable (as serializing Dictionary is a pain and
			worsen runtime performance, at least on load) */
			for(int j=0; j<darkMaterialMap.count(); ++j){
				if(material==darkMaterialMap[j].matNormal || material==darkMaterialMap[j].matDark){
					lDarkMaterialMap.Add(new DarkMaterialIndexMap(aRenderer[i],j));
					break;
				}
			}
		}
		layerNormal = gameObject.layer;
	}

	[CustomEditor(typeof(AmbientToggler))]
	class AmbientTogglerEditor : MonoBehaviourBakerEditor{
		AmbientToggler targetAs;
		protected override void OnEnable(){
			base.OnEnable();
			targetAs = (AmbientToggler)target;
		}
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			if(GUILayout.Button("Set Dark")){
				targetAs.setDarkAllChildren(true); }
			if(GUILayout.Button("Clear Dark")){
				targetAs.setDarkAllChildren(false); }
			EditorGUILayout.Space(5.0f);
			if(GUILayout.Button("Refresh Material Map")){
				targetAs.refreshMaterialMap(); }
		}
	}
	#endif
}
