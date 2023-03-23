#if UNITY_EDITOR

using UnityEngine;
using Chameleon;
using UnityEditor;

public class TestCollapseHierarchy : MonoBehaviour{
	void OnValidate(){
		Selection.selectionChanged -= takeOverChildSelection;
		Selection.selectionChanged += takeOverChildSelection;
	}
	private void takeOverChildSelection(){
		//if(!GetComponent<TestCollapseHierarchy>()){
		//	Selection.selectionChanged -= takeOverChildSelection; }
		Debug.Log("Here");
		Transform t = Selection.activeTransform;
		while(t){
			Debug.Log(t);
			t = t.parent;
			if(t == transform){
				Debug.Log("==");
				EditorApplication.update += ()=>{
					EditorHelper.setExpandInHierarchy(t.gameObject,false);
					Selection.activeTransform = transform;
				};
				return;
			}
		}
	}
	//private void unsubscribe(){
	//	Selection.selectionChanged -= takeOverChildSelection;
	//}
}

//[CustomEditor(typeof(TestCollapseHierarchy))]
//class TestCollapseHierarchyEditor : Editor{
//	private TestCollapseHierarchy targetAs;

//	void OnEnable(){
//		targetAs = target as TestCollapseHierarchy;
//	}
//	public override void OnInspectorGUI(){
//		//Debug.Log(EditorWindow.focusedWindow);
//		DrawDefaultInspector();
//		if(GUILayout.Button("Click")){
//			//Debug.Log(typeof(Object).GetField("m_Handle",ReflectionHelper.BINDINGFLAGS_ALL));
//			////EditorWindow.GetWindow(
//			//EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
//			//Selection.activeObject = ((TestCollapseHierarchy)target).gameObject;
//			//Event evt = new Event{
//			//	keyCode = KeyCode.LeftArrow,
//			//	type = EventType.KeyDown
//			//};
//			//EditorWindow.focusedWindow.SendEvent(evt);
//		}
//	}
//}

#endif