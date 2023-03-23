using UnityEngine;
using System.Collections;
using Chameleon;
using UnityEngine.UI;

public class PickableInspectable : OptionInspectable{
	[SerializeField] bool bMultiplePick;
	[SerializeField] AudioPrefab sfxpfPick;
	protected override void onOptionAction(){
		inspectionState = eInspectionState.EndSequence;
		routineInteract.start(this,rfActionSequence());
	}
	protected virtual IEnumerator rfActionSequence(){
		CursorController.ShowCursor = false;
		PlayerController playerController = PlayerController.Instance;
		DlgInteract dlgFooter = SceneMainManager.Instance.DlgFooter;
		Vector3 vWalkTarget =
			transform.position.newY(playerController.transform.position.y)-
			playerController.transform.forward*playerController.DistancePickup
		;
		yield return new ParallelEnumerator(this,
			PlayerController.Instance.rfWalkToward(
				vWalkTarget,
				Vector3.Distance(
					transform.position.xz(),
					PlayerController.Instance.transform.position.xz()
				) < playerController.DistancePickup ?
					PlayerController.eCutsceneMotion.WalkBackward :
					PlayerController.eCutsceneMotion.Walk
			),
			dlgFooter.prepareItrTweenDlg(true)
		);
		dlgFooter.close(true);
		yield return PlayerController.Instance.rfPickup();
		yield return rfEndInspectSequence();
		if(!bMultiplePick){
			//SceneMainManager.Instance.CanvasBalloon.gameObject.SetActive(false);
			deactivate();
			this.enabled = false;
			gameObject.SetActive(false);
		}
	}
	public virtual void onPicked(){
		HeadLookController.Instance.setHeadLookTarget(null);
		SfxPlayer.Instance.play(sfxpfPick);
	}
}
