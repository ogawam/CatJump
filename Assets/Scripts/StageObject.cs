using UnityEngine;
using System.Collections;
using DG.Tweening;

public class StageObject : MonoBehaviour {

	public void Setup(StageDataObject data) {
		transform.localPosition = data.Pos;
		transform.localEulerAngles = Vector3.forward * data.Angle;
		switch (data.Move) {
		case StageDataObject.MoveType.Loop:
			transform.DOMove (data.MovePos, data.MoveSec);
			break;
		case StageDataObject.MoveType.Yoyo:
			transform.DOMove (data.MovePos, data.MoveSec).SetEase(Ease.InOutSine).SetRelative ().SetLoops (-1, LoopType.Yoyo);
			break;
		}
	}
}
