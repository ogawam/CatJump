using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : Singleton<GameManager> {

	[SerializeField] Camera _backCamera;
	[SerializeField] Camera _unitCamera;
	[SerializeField] Cat _cat;

	[SerializeField] float _onCameraCatHeight;
	[SerializeField] float _onCameraBossHeight;
	[SerializeField] float _catUpSpeedToCamera;
	[SerializeField] float _catDownSpeedToCamera;
	[SerializeField] float _bossAreaHeight;

	void Awake() {
		instance = this;
	}

	void Update() {
		float height = _cat.transform.localPosition.y;
		if (_cat.transform.localPosition.y > _bossAreaHeight) {
			_cat.OnKillMode ();
			height = _bossAreaHeight;
			height += _onCameraBossHeight;
		} else {
			height += _cat.Speed.y * (_cat.Speed.y > 0 ? _catUpSpeedToCamera : _catDownSpeedToCamera);
			height += _onCameraCatHeight;
		}
		float offset = height - _unitCamera.transform.localPosition.y;
		Vector3 pos = _unitCamera.transform.localPosition + (Vector3.up * offset * Time.deltaTime);
		pos.y = Mathf.Max (0, pos.y);
		_unitCamera.transform.localPosition = pos;
		if (_cat.transform.localPosition.y < -320) {
			Retry ();
		}
	}

	public void Retry() {
		SceneManager.LoadScene ("Main");
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube (Vector3.up * 1024, new Vector3(640, 2048 + 640));
	}
}
