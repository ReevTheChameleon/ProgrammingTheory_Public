#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class TestCopyObject : MonoBehaviour{
	static SerializedObject so;
	[MenuItem("CONTEXT/Component/Copy")]
	public static void copyCheck(){
		EditorApplication.ExecuteMenuItem("CONTEXT/Transform/Copy Component");
	}
}
#endif
