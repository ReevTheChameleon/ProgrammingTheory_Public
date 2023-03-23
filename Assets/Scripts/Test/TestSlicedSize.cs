using UnityEngine;
using Chameleon;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;

public class TestSlicedSize : MonoBehaviour{
	public Image image;
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestSlicedSize))]
class TestSlicedSizeEditor : Editor<TestSlicedSize>{
	public override void OnInspectorGUI(){
		base.OnInspectorGUI();
		if(GUILayout.Button("Click")){
			Sprite sprite = targetAs.image.sprite;
			Debug.Log(sprite.textureRect.width+" "+sprite.textureRect.height);
			Debug.Log(sprite.texture.width+" "+sprite.texture.height);
			Debug.Log(targetAs.image.mainTexture.width+" "+targetAs.image.mainTexture.height);
			Debug.Log(sprite.packed);
			Debug.Log(sprite.rect.width+" "+sprite.rect.height);
		}
	}
}
#endif
