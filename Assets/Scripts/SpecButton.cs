using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpecButton : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
{

	public Nescafe.Controller.Button type;
	UnityEngine.UI.Button button;

	public void OnPointerDown(PointerEventData eventData)
	{
		UNES.PressButton (type, true);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		UNES.PressButton (type, false);
	}

	void OnDisable()
	{
		UNES.PressButton (type, false);
	}

}
