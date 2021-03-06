﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameUnitWorld : Singleton<GameUnitWorld> {

	BoxCollider2D[] _boxColliders;
	[SerializeField] float _scaffoldThickness;

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
		public bool isHit = false;
		public Vector2 setPos; 
		public Vector2 hitPos;
		public Vector2 hitVec;
		public Collider2D landingObject = null;
		public EnemyBase.Result enemyResult = EnemyBase.Result.None;
	}

	public Result CheckCircle(Vector2 checkPos, float checkRadius, Vector2 checkVec) {
		Result result = new Result ();
			
		return result;
	}

	public Result CheckBox(Vector2 checkPos, Vector2 checkVec, BoxCollider2D checkBox, bool isRolling) {
		Result result = new Result();
		Vector2 resultPos = Vector2.zero;
		int xHitNum = 0;
		int yHitNum = 0;

		checkPos += checkBox.offset;
		Vector2 checkSize = checkBox.size;

		foreach(BoxCollider2D boxCollider in _boxColliders) {
			if (boxCollider == null || !boxCollider.enabled)
				continue; 
			
			if (checkBox.tag == boxCollider.tag || boxCollider.tag == "Player")
				continue;

			Vector2 pos = boxCollider.transform.localPosition;
			pos += boxCollider.offset;
			Vector2 size = checkSize + boxCollider.size;
			Vector2 vec = pos - checkPos;

			// 足場の場合
			if (boxCollider.tag == "Scaffold") {
				if (checkVec.y > 0 || (vec.y + checkVec.y) > -(size.y / 2 - _scaffoldThickness)) {
					continue;
				}
			}

			for (int loop = 0; loop < 2; ++loop) {

				if (vec.x < size.x / 2 && vec.x > -size.x / 2 &&
				    vec.y < size.y / 2 && vec.y > -size.y / 2) {
					if (boxCollider.tag == "Enemy") {
						EnemyBase enemy = boxCollider.GetComponentInParent<EnemyBase> ();
						if (enemy != null) {
							result.hitPos = pos;
							EnemyBase.Result enemyResult = enemy.CheckHit (vec, isRolling);
							if (enemyResult > result.enemyResult) {
								result.enemyResult = enemyResult;
							}
							if (enemyResult == EnemyBase.Result.Defeat)
								break;
						}
					}

					//				Debug.Log ("vec "+ vec+ " size "+ size+ " checkVec "+ checkVec);
					if (Mathf.Abs (vec.y) > Mathf.Abs (vec.x) || boxCollider.tag == "Scaffold") {
						if (vec.y > 0) {
							resultPos.y += pos.y - size.y / 2;
						} else {
							result.landingObject = boxCollider;
							resultPos.y += pos.y + size.y / 2;
						}
						yHitNum++;
					} else {
						if (vec.x > 0)
							resultPos.x += pos.x - size.x / 2;
						else
							resultPos.x += pos.x + size.x / 2;
						xHitNum++;
					}
					result.isHit = true;
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
		result.setPos = checkPos;
		if (xHitNum > 0) result.setPos.x = resultPos.x / xHitNum;
		if (yHitNum > 0) result.setPos.y = resultPos.y / yHitNum;
		result.hitVec = (result.setPos - checkPos).normalized;
		result.setPos -= checkBox.offset;
		return result;
	}
}
