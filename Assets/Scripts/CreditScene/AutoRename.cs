#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Chameleon;
using TMPro;

[RemoveOnBuild(bApplyInEditorBuild=true)]
class AutoRename : MonoBehaviour{
	private static readonly char[] aSeparator = new char[]{' ','_','-',','};
	private enum eNameSource{None,ChildName,TMP_Text}
	[SerializeField] string sPrefix;
	[SerializeField] eNameSource nameSource;
	[SerializeField] int charLimit = 20;
	[SerializeField] bool bDropSplicedLastWord = true;
	[SerializeField] string sSuffix;

	private void applyRename(){
		string sName = sPrefix;
		string sSource = "";
		switch(nameSource){
			case eNameSource.None:
				return;
			case eNameSource.ChildName:
				sSource = transform.GetChild(0)?.gameObject.name;
				break;
			case eNameSource.TMP_Text:
				sSource = GetComponentInChildren<TMP_Text>()?.text;
				break;
		}
		if(sSource.Length > charLimit){
			sSource = sSource.Substring(0,charLimit);
			if(bDropSplicedLastWord){
				int index = sSource.LastIndexOfAny(aSeparator);
				if(index >= 1){
					sSource = sSource.Substring(0,index);}
			};
		}
		sName += sSource;
		sName += sSuffix;
		gameObject.name = sName;
	}

	[CustomEditor(typeof(AutoRename))]
	[CanEditMultipleObjects]
	class AutoRenameEditor : Editor{
		public override void OnInspectorGUI(){
			DrawDefaultInspector();
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Apply All")){
				ObjectExtension.forAllObjectsOfType<AutoRename>(
					(m) => {m.applyRename();});}
			if(GUILayout.Button("Apply")){
				((AutoRename)target).applyRename(); }
			EditorGUILayout.EndHorizontal();
		}
	}
}


#endif
