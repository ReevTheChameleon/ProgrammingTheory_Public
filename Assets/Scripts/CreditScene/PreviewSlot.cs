using UnityEngine;
using Chameleon;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PreviewSlot : LoneMonoBehaviour<PreviewSlot>{
	[SerializeField] Image imgBackground;
	[SerializeField] Image imgPreview;
	[SerializeField] GameObject gDefault;
	[SerializeField] GameObject gMat;
	[SerializeField] TextMeshProUGUI txtPreview;
	[SerializeField] Slideshow slideshow;
	[SerializeField] Sprite sprAudio;
	[SerializeField] Color tintAudio;
	private MeshRenderer rendererMat;
	private Mesh meshMatOriginal;
	private Mesh meshMat;
	private RotateY rotateMatScript;

	protected override void Awake(){
		base.Awake();
		rendererMat = gMat.GetComponent<MeshRenderer>();
		MeshFilter meshfilterMat = gMat.GetComponent<MeshFilter>();
		meshMatOriginal = meshfilterMat.sharedMesh;
		meshMat = meshfilterMat.mesh;
		rotateMatScript = gMat.GetComponent<RotateY>();
	}
	void Start(){
		exitPreview();
	}
	public void clearSlot(){
		imgBackground.color = Color.clear;
		imgPreview.sprite = null;
		imgPreview.color = Color.clear;
		imgPreview.transform.localScale = Vector3.one;
		imgPreview.transform.position = transform.position;
		gDefault.SetActive(false);
		gMat.SetActive(false);
		txtPreview.enabled = false;
		slideshow.gameObject.SetActive(false);
	}
	public void previewSprite(Sprite sprite,float scale,
		Color colorBackground,Color colorTint)
	{
		gDefault.SetActive(false);
		imgBackground.color = colorBackground;
		imgPreview.sprite = sprite;
		imgPreview.color = colorTint;
		Vector2 spriteScale = new Vector2(sprite.texture.width,sprite.texture.height)
			/ Mathf.Max(sprite.texture.width,sprite.texture.height);
		imgPreview.transform.localScale =
			new Vector3(spriteScale.x,spriteScale.y,1.0f) * scale;

		//imgPreview.rectTransform.fitParent();
		//imgPreview.sprite = sprite;
		//imgPreview.resizeToMatchTextureAspectRatio();
		//imgPreview.transform.localScale = new Vector3(scale,scale,1.0f);
		//imgPreview.color = colorTint;
	}
	public void previewMaterial(Material material,Vector2 uv){
		gDefault.SetActive(false);
		gMat.SetActive(true);
		rendererMat.material = material;
		scaleMesh(uv);
		rotateMatScript.enabled = true;
	}
	public void setPreviewMaterialRotate(bool bRotate){
		rotateMatScript.enabled = bRotate;
	}
	private void scaleMesh(Vector2 uv){
		Vector2[] aUV = meshMatOriginal.uv;
		for(int i=0; i<aUV.Length; ++i){
			aUV[i].x *= uv.x;
			aUV[i].y *= uv.y;
		}
		meshMat.uv = aUV;
	}
	public void previewAudioSprite(){
		previewSprite(sprAudio,0.9f,Color.clear,tintAudio);
	}
	public void previewText(TMP_FontAsset txtFont,float size,Color colorTint){
		txtPreview.enabled = true;
		txtPreview.font = txtFont;
		txtPreview.fontSize = size;
		txtPreview.color = colorTint;
	}
	public void previewSlideshow(List<Sprite> lSprite,float size,int startIndex){
		slideshow.gameObject.SetActive(true);
		slideshow.transform.localScale = new Vector3(size,size,1.0f);
		slideshow.setLSprite(lSprite);
		slideshow.startIndex = startIndex;
		slideshow.start();
	}
	public void pauseSlideshow(){
		slideshow.pause();
	}
	public void resumeSlideshow(){
		slideshow.resume();
	}
	public int getCurrentSlideIndex(){
		return slideshow.Index;
	}
	public void exitPreview(){
		clearSlot();
		gDefault.SetActive(true);
	}
}
