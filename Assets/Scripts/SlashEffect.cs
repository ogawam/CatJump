using UnityEngine;
using DG.Tweening;
using System.Collections;

public class SlashEffect : MonoBehaviour {

	[SerializeField] Vector3 _offset;
	[SerializeField] float _time;

	// Use this for initialization
	void Start () {
		transform.localPosition -= _offset;
		transform.DOLocalMove (_offset * 2, _time).SetRelative().OnComplete(()=>{ Destroy(gameObject); });
	}
}
