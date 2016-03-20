using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Cat : MonoBehaviour {

	[SerializeField] Sprite[] _sprites;
	[SerializeField] SpriteRenderer _renderer;
	[SerializeField] SpriteRenderer _arrowRenderer;
	[SerializeField] BoxCollider2D _bodyCollider;

	[SerializeField] GameInput[] _inputs;

	[SerializeField] float _movePower;
	[SerializeField] float _jumpPower;
	[SerializeField] float _fallPower;
	[SerializeField] float _groundLine;
	[SerializeField] float _maxChargeSec;

	class Flag {
		static public readonly int InAir	= (1 << 0);
		static public readonly int Aerial	= (1 << 1);
		static public readonly int Dead		= (1 << 2);
		static public readonly int KillMode	= (1 << 3);
	}

	int _flag;

	enum State {
		Stand,
		MoveL,
		MoveR
	}

	enum InputType {
		Left = (1<<0),
		Right = (1<<1),
		Jump = (1<<2)
	}

	InputType _input = 0;
	float _inputRate = 0;
	float _chargeSec = 0;

	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 60;

		_renderer.sprite = _sprites [0];
		_inputs [0].Setup (OnMoveDown, OnMoveUp);
		_inputs [1].Setup (OnJumpDown, OnJumpUp);
	}

	Vector2 _speed = Vector2.zero;
	public Vector2 Speed { get { return _speed; } }
	
	// Update is called once per frame
	void Update () {
	#if UNITY_EDITOR
		PointerEventData eventData = new PointerEventData(null);
		if(Input.GetKey(KeyCode.A)) {
			eventData.position = Vector2.zero;
			OnMoveDown(eventData);
		}
		else if(Input.GetKeyUp(KeyCode.A)) {
			OnMoveUp(eventData);
		}
		if(Input.GetKey(KeyCode.D)) {
			eventData.position = Vector2.right * Screen.width * 0.375f;
			OnMoveDown(eventData);
		}
		else if(Input.GetKeyUp(KeyCode.D)) {
			OnMoveUp(eventData);
		}
		if(Input.GetKey(KeyCode.Space)) {
			OnJumpDown(eventData);
		}
		else if(Input.GetKeyUp(KeyCode.Space)) {
			OnJumpUp(eventData);
		}
	#endif

		if (_chargeSec > 0) {
			if (_chargeSec < _maxChargeSec * 0.25f)
				_renderer.sprite = _sprites [1];
			else if (_chargeSec < _maxChargeSec * 0.5f)
				_renderer.sprite = _sprites [2];
			else if (_chargeSec < _maxChargeSec * 0.75f)
				_renderer.sprite = _sprites [3];
			else {
				if (Utility.IsBit (_flag, Flag.KillMode))
					_renderer.sprite = _sprites [4];
				_renderer.transform.localPosition = Vector3.left * 3 * Mathf.Sin (Time.time * 20 * Mathf.PI);
			}
		}

		Vector2 pos = transform.localPosition;
		Vector3 scale = _renderer.transform.localScale;
		float move = 0;
		if( (_input & InputType.Jump) != 0) {
			_chargeSec = Mathf.Min (_chargeSec + Time.deltaTime, _maxChargeSec);
		}
		else {
			if ((_input & InputType.Left) != 0) {
				move -= _movePower;
				scale.x = -Mathf.Abs(scale.x);

			} else if ((_input & InputType.Right) != 0) {
				move += _movePower;
				scale.x = Mathf.Abs(scale.x);
			}
		}
		_speed.x += (move - _speed.x) * Mathf.Min(1, 2 * Time.deltaTime);
		_renderer.transform.localScale = scale;

		_speed.y -= _fallPower * Time.deltaTime;
		Vector2 vec = _speed * Time.deltaTime;
		pos += vec;
		if (pos.x < -Define.fieldWidth / 2)
			pos.x += Define.fieldWidth;
		else if(pos.x > Define.fieldWidth / 2)
			pos.x -= Define.fieldWidth;

		_arrowRenderer.transform.localScale = Vector3.one * _inputRate;

		if (Utility.IsBit (_flag, Flag.InAir | Flag.KillMode))
			_renderer.transform.localEulerAngles = Vector3.back * 360 * 10 * Time.time;
		else
			_renderer.transform.localEulerAngles = Vector3.zero;

		if (!Utility.IsBit(_flag, Flag.Dead)) {
			if (pos.y < _groundLine) {
				pos.y = _groundLine;
				_speed.y = 0;
				if (Utility.IsBit(_flag, Flag.InAir)) {
					_flag = Utility.OffBit(_flag, Flag.InAir);
					_renderer.sprite = _sprites [0];
				}
			}

			Vector2 bodyColliPos = pos + _bodyCollider.offset;
			Vector2 bodyColliSize = _bodyCollider.size;

			GameUnitWorld.Result result = GameUnitWorld.Get ().CheckPlayer (bodyColliPos, bodyColliSize, vec);
			switch (result.enemyResult) {
			case EnemyBase.Result.Defeat:
				_speed.y = Mathf.Max(_speed.y, 0);
				_flag = Utility.OnBit (_flag, Flag.Aerial);
				break;
			case EnemyBase.Result.Damage:
				_speed = Vector3.up * 10;
				_renderer.sprite = _sprites[5];
				_renderer.transform.localEulerAngles = Vector3.forward * 180f;
				_flag = Utility.OnBit (_flag, Flag.Dead);
				_input = 0;
				break;
			}

			if (result.hitNum > 0) {
				//			Debug.Log ("hit " + result.hitNum + " vec " + result.hitVec);
				pos = result.setPos - _bodyCollider.offset;
				if (result.hitVec.y > 0) {
					_speed.y = 0;
					if (Utility.IsBit (_flag, Flag.InAir)) {
						_flag = Utility.OffBit (_flag, Flag.InAir);
						_renderer.sprite = _sprites [0];
					}
				} else if (result.hitVec.y <= -0.5f) {
					_speed.y = 0;
				}
			} else if (_speed.y < 0) {
				_flag = Utility.OnBit (_flag, Flag.InAir);
			}
		}
		transform.localPosition = pos;
	}

	void OnMoveDown(PointerEventData eventData) {
		if (Utility.IsBit (_flag, Flag.Dead))
			return;
		
		float quater = Screen.width * 0.125f;
		_inputRate = (eventData.position.x - quater) / quater;

		if (_inputRate > 0)
			_inputRate += 0.25f;
		else _inputRate -= 0.25f;
		_inputRate = Mathf.Clamp(_inputRate, -1, 1);

		_input &= ~(InputType.Right | InputType.Left);
		if(eventData.position.x > quater)
			_input |= InputType.Right;
		else _input |= InputType.Left;
	}

	void OnMoveUp(PointerEventData eventData) {
		if (Utility.IsBit (_flag, Flag.Dead))
			return;
		
		_inputRate = 0;
		_input &= ~(InputType.Right | InputType.Left);
	}

	void OnJumpDown(PointerEventData eventData) {
		if (Utility.IsBit (_flag, Flag.Dead))
			return;
		
		_input |= InputType.Jump;
	}

	void OnJumpUp(PointerEventData eventData) {
		if (Utility.IsBit (_flag, Flag.Dead))
			return;
		
		_input &= ~InputType.Jump;
		if (Utility.IsBit(_flag, Flag.InAir) 
		&& !Utility.IsBit(_flag, Flag.Aerial)) {
			_chargeSec = 0;
			return;
		}

		if(Utility.IsBit(_flag, Flag.KillMode))
			_renderer.sprite = _sprites [6];
		else _renderer.sprite = _sprites [5];
		_speed.y = _jumpPower * (_chargeSec / _maxChargeSec);
		_chargeSec = 0;
		_flag = Utility.OffBit (Utility.OnBit (_flag, Flag.InAir), Flag.Aerial);
	}

	public void OnKillMode() {
		_flag = Utility.OnBit(_flag, Flag.KillMode);
	}
}
