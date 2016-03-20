using UnityEngine;
using System.Collections;

static public class Utility {

	static public bool IsBit (int flag, int bit) {
		return (flag & bit) == bit;
	}

	static public int OnBit(int flag, int bit) {
		return flag | bit;
	}

	static public int OffBit(int flag, int bit) {
		return flag & ~bit;
	}
}
