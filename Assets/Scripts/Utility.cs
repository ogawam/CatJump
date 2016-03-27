using UnityEngine;
using System.Collections;

static public class Utility {

	static public bool Is (int flag, int bit) {
		return (flag & bit) == bit;
	}

	static public void On(ref int flag, int bit) {
		flag |= bit;
	}

	static public void Off(ref int flag, int bit) {
		flag &= ~bit;
	}
}
