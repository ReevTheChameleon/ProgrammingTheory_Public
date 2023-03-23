using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Chameleon;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct DarkMaterialPair{
	public Material matNormal;
	public Material matDark;
	public Material getMaterial(bool bDark){
		return bDark ? matDark : matNormal;
	}

	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(DarkMaterialPair))]
	class DarkMaterialPairDrawer : PropertyDrawer{
		public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
			if(typeof(IList).IsAssignableFrom(fieldInfo.FieldType)){
				position.width /= 2;
				position.width -= 10.0f;
				float savedLabelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = Mathf.Min(position.width/2,40.0f);
				EditorGUI.PropertyField(position,property.FindPropertyRelative(nameof(matNormal)),GUIContent.none);
				position.x += position.width+20.0f;
				EditorGUI.PropertyField(position,property.FindPropertyRelative(nameof(matDark)),GUIContent.none);
				EditorGUIUtility.labelWidth = savedLabelWidth;
			}
			else{
				float widthSlot = (position.width-EditorGUIUtility.labelWidth)/2-10.0f;
				position.width = EditorGUIUtility.labelWidth+widthSlot;
				EditorGUI.PropertyField(position,property.FindPropertyRelative(nameof(matNormal)),label);
				position.x += position.width+10.0f;
				position.width = widthSlot;
				EditorGUI.PropertyField(position,property.FindPropertyRelative(nameof(matDark)),GUIContent.none);
			}
		}
	}
	#endif
}

public class DarkMaterialMap : LoneMonoBehaviour<DarkMaterialMap>{
	[SerializeField] List<DarkMaterialPair> lMaterialPair;
	public DarkMaterialPair this[int index]{ get{return lMaterialPair[index];} }
	public int count(){ return lMaterialPair.Count; }

	#if UNITY_EDITOR
	[CustomEditor(typeof(DarkMaterialMap))]
	class DarkMaterialMapEditor : Editor{
		DarkMaterialMap targetAs;
		void OnEnable(){
			targetAs = (DarkMaterialMap)target;
		}
		public override void OnInspectorGUI(){
			DrawDefaultInspector();
			if(GUILayout.Button("Remove Duplicate")){
				/* Might be good if we can use HashSet instead, but we want to be able to
				control the order of material (maintenance purpose), and HashSet can't be
				serialized, so we just use it to clear duplicate. */
				HashSet<DarkMaterialPair> hashset = new HashSet<DarkMaterialPair>();
				for(int i=targetAs.lMaterialPair.Count-1; i>=0; --i){
					if(!hashset.Add(targetAs.lMaterialPair[i])){
						targetAs.lMaterialPair.RemoveAt(i);}
				}
			}
		}
	}
	#endif
}
