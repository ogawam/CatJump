using UnityEngine;
using System.Collections;

public class EnemyBoar : EnemyBoss {

	[SerializeField] float _gravity;

	enum State {
		Wait,
		Ready,
		Dash,
		Stop,
	}

	State _state = State.Wait;
	float _stateSec = 0;

	public class Flag {
		static public readonly int Landing = (1 << 0);
	}

	int _flag = 0;

	// Use this for initialization
	void Start () {
		Utility.On (ref _flag, Flag.Landing); 
	}

	Vector2 _speed = Vector2.zero;
	
	// Update is called once per frame
	void Update () {
		_stateSec += Time.deltaTime;
		Vector2 pos = transform.localPosition;
		Vector2 vec = _speed * Time.deltaTime;
		pos += vec;

		_speed.y += _gravity * Time.deltaTime;

		GameUnitWorld.Result result = GameUnitWorld.Get().CheckBox(pos, vec, _collider, false);
		if (result.isHit) {
			pos = result.setPos;
			if(result.hitVec.y > 0)
				_speed.y = 0;
		}

		switch (_state) {
		case State.Ready:
			if (_stateSec < 1f) {
				_renderer.transform.localPosition = Vector3.left * Mathf.Sin (Time.time * 360 * 5);
			} else {
				if (pos.x > 0)
					_speed.x = -320f;
				else
					_speed.x = 320f;
				SetState (State.Dash);
			}
			break;
		case State.Dash:
			if (result.isHit && Mathf.Abs (result.hitVec.x) > 0.5f) {
				GameManager.Get ().Shake ();
				_speed.x = -_speed.x * 0.25f;
				_speed.y = 240f;
				SetState (State.Stop);
			}
			break;
		case State.Stop:
			if (_stateSec < 2) {
				_speed.x -= _speed.x * 2 * Time.deltaTime;
			} else {
				if (pos.x > 0) {
					_renderer.transform.localScale = new Vector3 (1, 1, 1);
				} else {
					_renderer.transform.localScale = new Vector3 (-1, 1, 1);
				}
				SetState (State.Ready);
			}
			break;
		}

		transform.localPosition = pos;
	}

	public override Result CheckHit(Vector3 vec, bool isRolling) {
		Result result = Result.Damage;
		if (vec.y < 0) {
			if (isRolling)
				result = Damage () ? Result.Regist : Result.Defeat;
			else
				result = Result.Regist;
		} else {
			result = Result.Damage;
		}
		return result;
	}

	void SetState(State state) {
		_state = state;
		_stateSec = 0;
	}

	public override void Fight() {
		if (_state != State.Wait)
			return;
		
		SetState (State.Ready);
	}
}
