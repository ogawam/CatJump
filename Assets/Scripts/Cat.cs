using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Cat : MonoBehaviour {

	[SerializeField] Sprite[] _sprites;
	[SerializeField] SpriteRenderer _renderer;
	[SerializeField] SpriteRenderer _arrowRenderer;
	[SerializeField] BoxCollider2D _bodyCollider;
	[SerializeField] GameObject _prefabSlashEffect;

	[SerializeField] GameInput[] _inputs;

	[SerializeField] float _movePower;
	[SerializeField] float _jumpPower;
	[SerializeField] float _fallPower;
	[SerializeField] float _rollingPower;
	[SerializeField] float _groundLine;
	[SerializeField] float _maxChargeSec;

	[SerializeField] float _maxRollingSec;
	float _rollingSec;

	Collider2D _landingObject = null;
	Vector2 _landingPosition = Vector2.zero;

	class Flag {
		static public readonly int InAir	= (1 << 0);
		static public readonly int Aerial	= (1 << 1);
		static public readonly int Dead		= (1 << 2);
		static public readonly int KillMode	= (1 << 3);
		static public readonly int Rolling	= (1 << 4);
	}

	int _flag;

	enum State {
		Stand,
		MoveL,
		MoveR
	}

	class InputType {
		static public readonly int Left = (1 << 0);
		static public readonly int Right = (1 << 1);
		static public readonly int Jump = (1 << 2);
	}

	int _inputType = 0;
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
				if (IsReadyToRolling)
					_renderer.sprite = _sprites [4];
				_renderer.transform.localPosition = Vector3.left * 3 * Mathf.Sin (Time.time * 20 * Mathf.PI);
			}
		}

		if(!Utility.Is(_flag, Flag.Rolling))
			_speed.y -= _fallPower * Time.deltaTime;

		Vector2 pos = transform.localPosition;
		Vector2 vec = _speed * Time.deltaTime;
		if (_landingObject != null) {
			Vector2 nextPos = _landingObject.transform.localPosition;
			pos += nextPos - _landingPosition;
			_landingPosition = nextPos;
		}
		pos += vec;
		if (pos.x < -Define.fieldWidth / 2)
			pos.x += Define.fieldWidth;
		else if(pos.x > Define.fieldWidth / 2)
			pos.x -= Define.fieldWidth;

		if (Utility.Is (_flag, Flag.Dead)) {
			_renderer.sprite = _sprites [5];
			_renderer.transform.localEulerAngles = Vector3.forward * 180f;
			_arrowRenderer.transform.localScale = Vector3.zero;
		}
		else {

			if (Utility.Is (_flag, Flag.Rolling)) {
				_arrowRenderer.transform.localScale = Vector3.zero;
				_renderer.transform.localEulerAngles = Vector3.back * 360 * 10 * Time.time;
				_rollingSec += Time.deltaTime;
				if (_rollingSec > _maxRollingSec) {
					_renderer.sprite = _sprites[5];
					_renderer.transform.localEulerAngles = Vector3.zero;
					Utility.Off (ref _flag, Flag.Rolling);
				}				
			} else {
				if (IsReadyToRolling) {
					_arrowRenderer.transform.localScale = Vector3.one;
					_arrowRenderer.transform.localEulerAngles = Vector3.forward * (90 - 45 * _inputRate);
				} else {
					_arrowRenderer.transform.localScale = Vector3.one * _inputRate;
					_arrowRenderer.transform.localEulerAngles = Vector3.zero;
				}
				Vector3 scale = _renderer.transform.localScale;
				float move = 0;
				if (Utility.Is (_inputType, InputType.Jump)) {
					_chargeSec = Mathf.Min (_chargeSec + Time.deltaTime, _maxChargeSec);
				} else {
					if (Utility.Is (_inputType, InputType.Left)) {
						move -= _movePower;
						scale.x = -Mathf.Abs (scale.x);

					} else if (Utility.Is (_inputType, InputType.Right)) {
						move += _movePower;
						scale.x = Mathf.Abs (scale.x);
					}
				}
				_speed.x += (move - _speed.x) * Mathf.Min (1, 2 * Time.deltaTime);
				_renderer.transform.localScale = scale;
				_renderer.transform.localEulerAngles = Vector3.zero;
			}

			if (pos.y <= _groundLine) {
				pos.y = _groundLine;
				_speed.y = 0;
				if (Utility.Is(_flag, Flag.InAir)) {
					Utility.Off(ref _flag, Flag.InAir);
					_renderer.sprite = _sprites [0];
				}
			}

			GameUnitWorld.Result result = GameUnitWorld.Get ().CheckBox (pos, vec, _bodyCollider, Utility.Is (_flag, Flag.Rolling));
			GameObject effect = null;
			switch (result.enemyResult) {
			case EnemyBase.Result.Defeat:
				effect = Instantiate (_prefabSlashEffect, result.hitPos, Quaternion.identity) as GameObject;
				if (!Utility.Is (_flag, Flag.Rolling)) {
					if (_speed.y < 0)
						_speed.y = -Speed.y;
					Utility.On (ref _flag, Flag.Aerial);
				}
				break;
			case EnemyBase.Result.Regist:
				effect = Instantiate (_prefabSlashEffect, result.hitPos, Quaternion.identity) as GameObject;
				if (!Utility.Is (_flag, Flag.Rolling)) {
					if (_speed.y < 0)
						_speed.y = Mathf.Max(320f, -Speed.y * 0.5f);
					else _speed.y = 0;
				}
				break;
			case EnemyBase.Result.Damage:
				_speed = Vector3.up * 10;
				_renderer.sprite = _sprites [5];
				_renderer.transform.localEulerAngles = Vector3.forward * 180f;
				Utility.On (ref _flag, Flag.Dead);
				_inputType = 0;
				break;
			}

			if (effect != null) {
				effect.transform.SetParent (GameUnitWorld.Get().transform, false);
			}

			if (result.isHit) {
				//			Debug.Log ("hit " + result.hitNum + " vec " + result.hitVec);
				pos = result.setPos;
				if(Utility.Is(_flag, Flag.Rolling)) {
					float angle = Mathf.Atan2 (result.hitVec.x, result.hitVec.y) * Mathf.Rad2Deg;
					Quaternion rotation = Quaternion.Euler (0, 0, angle);
					_speed = rotation * _speed;
					_speed.y = -Speed.y;
					_speed = Quaternion.Inverse (rotation) * _speed;
					Debug.Log ("angle "+ angle);
				}
				else if(result.enemyResult == EnemyBase.Result.None) {
					if (result.hitVec.y > 0) {
						_speed.y = 0;
						if (Utility.Is (_flag, Flag.InAir)) {
							Utility.Off (ref _flag, Flag.InAir);
							_renderer.sprite = _sprites [0];
							_landingObject = result.landingObject;
							_landingPosition = _landingObject.transform.localPosition;
							Debug.Log ("Landing");
						}
					} else if (result.hitVec.y <= -0.5f) {
						_speed.y = 0;
					}
				}
			} else if (_speed.y < 0) {
				Utility.On (ref _flag, Flag.InAir);
				_landingObject = null;
			}
		}
		transform.localPosition = pos;
	}

	void OnMoveDown(PointerEventData eventData) {
		if (Utility.Is (_flag, Flag.Dead))
			return;
		
		float quater = Screen.width * 0.125f;
		_inputRate = (eventData.position.x - quater) / quater;

		if (_inputRate > 0)
			_inputRate += 0.25f;
		else
			_inputRate -= 0.25f;
		_inputRate = Mathf.Clamp (_inputRate, -1, 1);

		Utility.Off(ref _inputType, InputType.Right | InputType.Left);
		if(eventData.position.x > quater)
			Utility.On(ref _inputType, InputType.Right);
		else Utility.On(ref _inputType, InputType.Left);
	}

	void OnMoveUp(PointerEventData eventData) {
		if (Utility.Is (_flag, Flag.Dead))
			return;
		
		_inputRate = 0;
		Utility.Off(ref _inputType, InputType.Right | InputType.Left);
	}

	void OnJumpDown(PointerEventData eventData) {
		if (Utility.Is (_flag, Flag.Dead))
			return;

		if (Utility.Is (_flag, Flag.Rolling))
			Utility.Off (ref _flag, Flag.Rolling);
		_inputType |= InputType.Jump;
	}

	void OnJumpUp(PointerEventData eventData) {
		if (Utility.Is (_flag, Flag.Dead))
			return;
		
		_inputType &= ~InputType.Jump;
		if (Utility.Is(_flag, Flag.InAir) 
		&& !Utility.Is(_flag, Flag.Aerial)) {
			_chargeSec = 0;
			return;
		}

		if (IsReadyToRolling) {
			Utility.On (ref _flag, Flag.Rolling);
			_rollingSec = 0;
			_renderer.sprite = _sprites [6];
			_speed.y = _rollingPower;
			_speed = Quaternion.Euler(0,0,-45 * _inputRate) * _speed;
		} else {
			_renderer.sprite = _sprites [5];
			_speed.y = _jumpPower * (_chargeSec / _maxChargeSec);
		}
		_chargeSec = 0;
		_landingObject = null;
		Utility.On (ref _flag, Flag.InAir);
		Utility.Off (ref _flag, Flag.Aerial);
	}

	public void OnKillMode() {
		Utility.On(ref _flag, Flag.KillMode);
	}

	bool IsReadyToRolling { get { return Utility.Is (_flag, Flag.KillMode) && _chargeSec >= _maxChargeSec; } }
}
