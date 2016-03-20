using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasManager : Singleton<CanvasManager> {

	void Awake () {
		instance = this;
	}
}
