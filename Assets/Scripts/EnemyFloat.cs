using UnityEngine;
using System.Collections;

public class EnemyFloat : EnemyBase {

	[SerializeField] Sprite[] _sprites;
	
	[SerializeField] float _heightToJump;
	[SerializeField] float _jumpOffset;
	[SerializeField] float _jumpSpeed;
	[SerializeField] float _fallSpeed;
	[SerializeField] float _registance;
	Vector3 firstPosition;
	Vector3 _speed;

	// Use this for initialization
	void Start () {
		firstPosition = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		if (_hp > 0) {
			_speed.y += _fallSpeed * Time.deltaTime;
			_speed += _speed * _registance * Time.deltaTime;
			transform.localPosition += _speed * Time.deltaTime;
			if (_speed.y < 0) {
				_renderer.sprite = _sprites [0];
				if (transform.localPosition.y - firstPosition.y < _heightToJump) {
					Vector3 pos = firstPosition + Quaternion.Euler (0, 0, 360 * Random.value) * Vector3.up * _jumpOffset; 
					_speed = (pos - transform.localPosition).normalized * _jumpSpeed;
					_renderer.sprite = _sprites [1];
				}
			}
			float speedToAngle = Mathf.Rad2Deg * (Mathf.Atan2 (_speed.y, _speed.x)) * (_speed.y > 0 ? 1f : -1f);
			_renderer.transform.localEulerAngles = Vector3.forward * (speedToAngle - 90) * (_speed.y / _jumpSpeed);
		}
	}

	public override Result CheckHit (Vector3 vec)
	{
		Result result = Result.Damage;
		if (Mathf.Abs (vec.y) > Mathf.Abs (vec.x)) {
			if (Damage ())
				result = Result.Regist;
			else
				result = Result.Defeat;
		}
		return result;
	}
}
