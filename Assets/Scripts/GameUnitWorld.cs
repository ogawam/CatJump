using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameUnitWorld : Singleton<GameUnitWorld> {

	BoxCollider2D[] _boxColliders;

	void Awake() {
		instance = this;
	}

	void Start() {
		_boxColliders = GetComponentsInChildren<BoxCollider2D> ();
	}

	void Update() {
		_boxColliders = GetComponentsInChildren<BoxCollider2D> ();
	}

	public class Result {
		public int hitNum = 0;
		public Vector2 setPos; 
		public Vector2 hitPos;
		public Vector2 hitVec;
		public EnemyBase.Result enemyResult = EnemyBase.Result.None;
	}

	public Result CheckCircle(Vector2 checkPos, float checkRadius, Vector2 checkVec) {
		Result result = new Result ();
			
		return result;
	}

	public Result CheckPlayer(Vector2 checkPos, Vector2 checkSize, Vector2 checkVec) {
		Result result = new Result();
		Vector2 resultPos = Vector2.zero;
		foreach(BoxCollider2D boxCollider in _boxColliders) {
			if (boxCollider == null || !boxCollider.enabled)
				continue; 
			
			if (boxCollider.tag == "Player")
				continue;

			Vector2 pos = boxCollider.transform.localPosition;
			pos += boxCollider.offset;
			Vector2 size = checkSize + boxCollider.size;
			Vector2 vec = pos - checkPos;

			// 足場の場合
			if (boxCollider.tag == "Scaffold") {
				if (checkVec.y > 0 || (vec.y + checkVec.y) > -size.y / 2)
					continue;
			}

			for (int loop = 0; loop < 2; ++loop) {

				if (vec.x < size.x / 2 && vec.x > -size.x / 2 &&
				    vec.y < size.y / 2 && vec.y > -size.y / 2) {
					if (boxCollider.tag == "Enemy") {
						EnemyBase enemy = boxCollider.GetComponentInParent<EnemyBase> ();
						if (enemy != null) {
							EnemyBase.Result enemyResult = enemy.CheckHit (vec);
							if (enemyResult > result.enemyResult) {
								result.enemyResult = enemyResult;
							}
							if (enemyResult == EnemyBase.Result.Defeat)
								break;
						}
					}

					//				Debug.Log ("vec "+ vec+ " size "+ size+ " checkVec "+ checkVec);
					if (Mathf.Abs (vec.y) > Mathf.Abs (vec.x) || boxCollider.isTrigger) {
						if (vec.y > 0)
							pos.y -= size.y / 2 + 0.05f;
						else
							pos.y += size.y / 2 + 0.05f;
						pos.x = checkPos.x;
					} else {
						if (vec.x > 0)
							pos.x -= size.x / 2 + 0.05f;
						else
							pos.x += size.x / 2 + 0.05f;
						pos.y = checkPos.y;
					}
					resultPos += pos;
					result.hitNum++;
					break;
				}
				if (pos.x - size.x / 2 < -Define.fieldWidth / 2)
					pos.x += Define.fieldWidth;
				else if (pos.x + size.x / 2 > Define.fieldWidth / 2)
					pos.x -= Define.fieldWidth;
				else
					break;
				vec = pos - checkPos;
			}
		}
		if (result.hitNum > 0) {
			result.setPos = resultPos / result.hitNum;
			result.hitVec = (result.setPos - checkPos).normalized;
		}

		return result;
	}
}
