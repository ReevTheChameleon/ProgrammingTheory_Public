using UnityEngine;
using System.Collections;
using Chameleon;

public abstract class OptionInspectable : Inspectable{
	[SerializeField] string sDlgMessage;
	[SerializeField] string sDlgBtn1;
	[SerializeField] string sDlgBtn2;

	public override void onInteracted(){
		switch(inspectionState){
			case eInspectionState.None:
				SfxPlayer.Instance.play(sfxpfInspect);
				routineInteract.start(this,rfStartOptionInspcetSequence());
				break;
			case eInspectionState.FooterTexting:
				FooterManager.Instance.stepFooter();
				break;
			case eInspectionState.Suspended:
			case eInspectionState.EndSequence:
				break; //do nothing
		}
	}
	protected virtual IEnumerator rfStartOptionInspcetSequence(){
		CursorController.ShowCursor = true;
		yield return rfStartInspectSequence();
		DlgInteract dlgFooter = SceneMainManager.Instance.DlgFooter;
		dlgFooter.popup(
			Vector2.zero,
			sDlgMessage,
			sDlgBtn1,
			sDlgBtn2,
			onOptionAction,
			onOptionCancel,
			onOptionCancel
		);
		yield return dlgFooter.prepareItrTweenDlg(false);
	}
	protected virtual IEnumerator rfCancelOptionInspectSequence(){
		CursorController.ShowCursor = false;
		DlgInteract dlgFooter = SceneMainManager.Instance.DlgFooter;
		yield return new ParallelEnumerator(this,
			dlgFooter.prepareItrTweenDlg(true),
			rfEndInspectSequence()
		);
		dlgFooter.close(true);
	}
	protected virtual void onOptionCancel(){
		routineInteract.start(this,rfCancelOptionInspectSequence());
	}
	protected abstract void onOptionAction();
}
