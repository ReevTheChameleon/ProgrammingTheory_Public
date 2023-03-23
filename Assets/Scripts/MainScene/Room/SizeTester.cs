using UnityEngine;
using Chameleon;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteOnBuild(nameof(destroyGameObject),bApplyInEditorBuild=true)]
public class SizeTester : MonoBehaviour{
	[SerializeField] BoxCollider[] abcItem;
	[SerializeField] Vector2[] av2Pos;
	[SerializeField] SceneMainManager sceneMainManager;

	private void destroyGameObject(){
		DestroyImmediate(gameObject);
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(SizeTester))]
	class SizeTesterEditor : Editor{
		SizeTester targetAs;
		private Vector2 v2Size;
		void OnEnable(){
			targetAs = (SizeTester)target;
			v2Size = Vector2.zero;
			for(int i=0; i<targetAs.abcItem.Length; ++i){
				BoxCollider bc = targetAs.abcItem[i];
				if(!bc){
					continue;}
				v2Size = new Vector2(
					Mathf.Max(v2Size.x,bc.size.x*bc.transform.localScale.x),
					Mathf.Max(v2Size.y,bc.size.z*bc.transform.localScale.z)
				);
			}
		}
		public override void OnInspectorGUI(){
			DrawDefaultInspector();
			if(GUILayout.Button("Submit to SceneMainManager")){
				targetAs.sceneMainManager.setItemPositionTable(targetAs.av2Pos);
			}
		}
		void OnSceneGUI(){
			Transform transform = targetAs.transform;
			for(int i=0; i<targetAs.av2Pos.Length; ++i){
				Vector3 vPos =
					transform.position +
					new Vector3(targetAs.av2Pos[i].x,transform.position.y,targetAs.av2Pos[i].y)
				;
				Handles.DrawWireCube(vPos,new Vector3(v2Size.x,1.0f,v2Size.y));
				EditorGUI.BeginChangeCheck();
				Vector3 vPosUser = Handles.PositionHandle(vPos,Quaternion.identity);
				if(EditorGUI.EndChangeCheck()){
					Undo.RecordObject(target,"Move SizeTester");
					targetAs.av2Pos[i] = (vPosUser-transform.position).xz();
				}
			}
		}
	}
	#endif
}

