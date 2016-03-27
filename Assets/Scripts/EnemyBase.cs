using UnityEngine;
using System.Collections;
using DG.Tweening;

public abstract class EnemyBase : MonoBehaviour {

	[SerializeField] protected SpriteRenderer _renderer;
	[SerializeField] protected BoxCollider2D _collider;
	[SerializeField] protected int _hp;

	public enum Result {
		None,
		Regist,
		Defeat,
		Damage
	}

	public abstract Result CheckHit(Vector3 vec, bool isRolling);
	protected bool Damage() {
		if(_hp > 0) {
			_hp--;
			if (_hp > 0) {
				_renderer.transform.DOShakePosition (0.5f, 16);
			} else {
				_collider.enabled = false;
				_renderer.DOFade (0, 0.2f).OnComplete (() => {
					Destroy (gameObject);
				});
			}
		}
		return _hp > 0;
	}
}
