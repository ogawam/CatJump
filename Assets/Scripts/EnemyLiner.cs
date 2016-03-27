using UnityEngine;
using System.Collections;

public class EnemyLiner : EnemyBase {

	[SerializeField] float _speed;
	[SerializeField] bool _damage;
	Collider2D _landingObject = null;
	float _landingOffset = 0;
	float _direction = 1;

	// Use this for initialization
	void Start () {
		GameUnitWorld.Result result = GameUnitWorld.Get ().CheckBox(transform.localPosition, Vector2.down * 100, _collider, false);
		if (result.landingObject != null) {
			_landingObject = result.landingObject;
			transform.localPosition = result.setPos;
			_landingOffset = transform.localPosition.x - _landingObject.transform.localPosition.x;
		}
	}
	
	// Update is called once per frame
	void Update () {
		BoxCollider2D boxCollider = _landingObject as BoxCollider2D;
		if (boxCollider != null) {
			_landingOffset += _speed * _direction * Time.deltaTime;
			float halfWidth = boxCollider.size.x / 2;
			if (Mathf.Abs (_landingOffset) > halfWidth) {
				_landingOffset = Mathf.Clamp (_landingOffset, -halfWidth, halfWidth);
				_direction = -_direction;
				_renderer.flipX = (_direction < 0);
			}
			transform.localPosition = boxCollider.transform.localPosition + Vector3.right * _landingOffset;
		}
	}

	public override Result CheckHit (Vector3 vec, bool isRolling)
	{
		Result result = Result.Damage;
		if (_damage) {
			if (Mathf.Abs(vec.y) > Mathf.Abs (vec.x)) {
				if (vec.y > 0) {
					if (Damage ())
						result = Result.Regist;
					else
						result = Result.Defeat;
				} else {
					result = Result.Regist;
				}
			}
		}
		return result;
	}
}
