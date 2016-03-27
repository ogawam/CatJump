using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StageDataObject {
	public enum DataType {
		Scaffold,
		Block,
	}

	public enum MoveType {
		None,
		Loop,
		Yoyo,
	}

	[SerializeField] DataType _type;
	public DataType Type { get { return _type; } }
	[SerializeField] int _size;
	public int Size { get { return _size; } }
	[SerializeField] Vector2 _pos;
	public Vector2 Pos { get { return _pos; } }
	[SerializeField] float _angle;
	public float Angle { get { return _angle; } }
	[SerializeField] MoveType _move;
	public MoveType Move { get { return _move; } }
	[SerializeField] Vector2 _movePos;
	public Vector2 MovePos { get { return _movePos; } }
	[SerializeField] float _moveSec;
	public float MoveSec { get { return _moveSec; } }

}

[System.Serializable]
public class StageDataEnemy {
	public enum DataType {
		Grounder,
		Floater,
		Liner,
	}

	[SerializeField] DataType _type;
	public DataType Type { get { return _type; } }
	[SerializeField] int _level;
	public int Level { get { return _level; } }
	[SerializeField] Vector2 _pos;
	public Vector2 Pos { get { return _pos; } }
}

public class StageData : ScriptableObject {

	[SerializeField] int _stageNo;
	public int StageNo { get { return _stageNo; } }
	[SerializeField] float _roadHeight;
	public float RoadHeight { get { return _roadHeight; } }
	[SerializeField] float _bossHeight;
	public float BossHeight { get { return _bossHeight; } }
	[SerializeField] Vector2 _bossPos;
	public Vector2 BossPos { get { return _bossPos; } }

	[SerializeField] List<StageDataObject> _objects;
	public List<StageDataObject> Objects { get { return _objects; } }
	[SerializeField] List<StageDataEnemy> _enemies;
	public List<StageDataEnemy> Enemies { get { return _enemies; } }
}
