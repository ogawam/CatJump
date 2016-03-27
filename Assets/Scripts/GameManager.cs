using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class GameManager : Singleton<GameManager> {

	[SerializeField] GameUnitWorld _gameUnitWorld;
	[SerializeField] Camera _backCamera;
	[SerializeField] Camera _unitCamera;
	[SerializeField] Cat _cat;

	[SerializeField] EnemyBase[] _prefabEnemyGrounders;
	[SerializeField] EnemyBase[] _prefabEnemyFloters;
	[SerializeField] EnemyBase[] _prefabEnemyLiners;
	[SerializeField] EnemyBoss[] _prefabEnemyBosses;
	[SerializeField] StageObject[] _prefabObjectScaffolds;
	[SerializeField] StageObject[] _prefabObjectBlocks;
	[SerializeField] StageData[] _stageDatas;

	[SerializeField] float _onCameraCatHeight;
	[SerializeField] float _onCameraBossHeight;
	[SerializeField] float _catUpSpeedToCamera;
	[SerializeField] float _catDownSpeedToCamera;
	[SerializeField] float _bossAreaHeight;

	StageData _stageData;
	EnemyBoss _boss;

	int _stageNo = 1;

	void Awake() {
		instance = this;
	}

	bool _initialized = false;
	IEnumerator Start() {
		yield return StartCoroutine(CreateStage (_stageDatas [_stageNo]));
	}

	void Update() {
		if (!_initialized)
			return;
		
		if (_stageNo > 0) {
			float height = _cat.transform.localPosition.y;
			if (_cat.transform.localPosition.y > _stageData.BossHeight) {
				_cat.OnKillMode ();
				height = _bossAreaHeight;
				height += _onCameraBossHeight;
				_boss.Fight ();
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
	}

	public void ToBossArea() {
		_cat.transform.localPosition = Vector3.up * (_bossAreaHeight + 32);
	}

	public void Retry() {
		SceneManager.LoadScene ("Main");
	}

	public void Shake() {
		_unitCamera.transform.DOShakePosition (1.0f, 32f);
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube (Vector3.up * 1024, new Vector3(640, 2048 + 640));
	}

	IEnumerator CreateStage(StageData stageData) {
		_initialized = false;
		_boss = Instantiate<EnemyBoss>(_prefabEnemyBosses [stageData.StageNo]);
		_boss.transform.localPosition = stageData.BossPos;
		_boss.transform.SetParent (_gameUnitWorld.transform, false);

		foreach (StageDataObject stageDataObject in stageData.Objects) {
			StageObject prefab = null;
			switch (stageDataObject.Type) {
			case StageDataObject.DataType.Scaffold:
				prefab = _prefabObjectScaffolds[stageDataObject.Size];
				break;
			case StageDataObject.DataType.Block:
				prefab = _prefabObjectBlocks[stageDataObject.Size];
				break;
			}
			if (prefab != null) {
				StageObject stageObject = Instantiate<StageObject> (prefab);
				stageObject.Setup (stageDataObject);
				stageObject.transform.SetParent (_gameUnitWorld.transform, false);
			}
		}

		yield return 0;
		yield return 0;

		foreach(StageDataEnemy stageDataEnemy in stageData.Enemies) {
			EnemyBase prefab = null;
			switch (stageDataEnemy.Type) {
			case StageDataEnemy.DataType.Grounder:
				prefab = _prefabEnemyGrounders [stageDataEnemy.Level];
				break;
			case StageDataEnemy.DataType.Floater:
				prefab = _prefabEnemyFloters [stageDataEnemy.Level];
				break;
			case StageDataEnemy.DataType.Liner:
				prefab = _prefabEnemyLiners [stageDataEnemy.Level];
				break;
			}
			if (prefab != null) {
				EnemyBase enemy = Instantiate<EnemyBase> (prefab);
				enemy.transform.localPosition = stageDataEnemy.Pos;
				enemy.transform.SetParent (_gameUnitWorld.transform, false);
			}
		}

		_stageData = stageData;
		_initialized = true;
	}
}
