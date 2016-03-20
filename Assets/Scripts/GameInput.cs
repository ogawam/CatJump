using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class GameInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler {

	public void Setup(UnityAction<PointerEventData> downEventData, UnityAction<PointerEventData> upEventData) {
		_downEventData = downEventData;
		_upEventData = upEventData;
	}

	UnityAction<PointerEventData> _downEventData;
	public void OnPointerDown (PointerEventData eventData) {
		_downEventData (eventData);
	}

	public void OnDrag (PointerEventData eventData) {
		_downEventData (eventData);
	}

	UnityAction<PointerEventData> _upEventData;
	public void OnPointerUp (PointerEventData eventData) {
		_upEventData (eventData);
	}

	public void OnEndDrag (PointerEventData eventData) {
		_upEventData (eventData);
	}
}
