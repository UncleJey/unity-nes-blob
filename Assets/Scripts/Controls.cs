using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour 
{
	Vector3 dest = Vector3.zero;	// Точка в пространстве куда идём
	public bool playing = false;	// Движется
	Vector3 mousePos = Vector3.zero;
	float downscale = -1f;
	float distance = 0;
	float lastMoveBack = 0f;

	public RectTransform cross;
	public Canvas canvas;

	Nescafe.Controller.Button lastKey = Nescafe.Controller.Button.No;

	void ChangeKey(Nescafe.Controller.Button pKey)
	{
		if (lastKey != Nescafe.Controller.Button.No && lastKey != pKey) 
		{
			UNES.PressButton (lastKey, false);
		}
		lastKey = pKey;
		if (pKey != Nescafe.Controller.Button.No)
			UNES.PressButton (lastKey, true);
	}

	void UnChangeKey(Nescafe.Controller.Button pKey)
	{
		if (lastKey == pKey)
		{
			UNES.PressButton (lastKey, false);
			lastKey = Nescafe.Controller.Button.No;
		}
	}

	void OnDisable()
	{
		ChangeKey(Nescafe.Controller.Button.No);
		Jump (false);
	}

	void MoveLeft()
	{
	#if UNITY_EDITOR
		Debug.Log("Left");
	#endif
		ChangeKey (Nescafe.Controller.Button.Left);
	}

	void MoveRight()
	{
	#if UNITY_EDITOR
		Debug.Log("Left");
	#endif
		ChangeKey (Nescafe.Controller.Button.Right);
	}

	void MoveUp()
	{
	#if UNITY_EDITOR
		Debug.Log("up");
	#endif
		ChangeKey (Nescafe.Controller.Button.Up);
	}

	void MoveDown()
	{
	#if UNITY_EDITOR
		Debug.Log("Down");
	#endif
		ChangeKey (Nescafe.Controller.Button.Down);
	}

	bool jumped = false;
	void Jump(bool isTapped)
	{
		if (isTapped && !jumped) 
		{
		#if UNITY_EDITOR
			Debug.Log("Jump");
		#endif
			UNES.PressButton (Nescafe.Controller.Button.A, true);
			jumped = true;
		}

		if (!isTapped && jumped) 
		{
		#if UNITY_EDITOR
			Debug.Log("unJump");
		#endif
			UNES.PressButton (Nescafe.Controller.Button.A, false);
			jumped = false;
		}
	}

	bool GetTouchPosition(out Vector2 vTouch)
	{
		vTouch = Vector2.zero;
		for (int i = Input.touchCount - 1; i >= 0; i--) 
		{
			vTouch = Input.touches [i].position;
			if (vTouch.x < (Screen.width >> 1))
				return true;
		}
		return false;
	}

	bool isTap
	{
		get 
		{
			for (int i = Input.touchCount - 1; i >= 0; i--) 
			{
				if (Input.touches [i].position.x > (Screen.width >> 1))
					return true;
			}
			return false;
		}
	}

	Vector2 crossPos = Vector2.zero;
	void Start()
	{
		crossPos =  cross.anchoredPosition * canvas.scaleFactor;
	}

	void Update () 
	{
		Vector2 touchPos;

		if (playing)
		{
			Jump(isTap);
			if (GetTouchPosition (out touchPos)) 
			{
				float dx=0, dxj=0;
				float dy=0, dyj=0;

				if (mousePos.x + mousePos.y == 0)
				{
					mousePos = touchPos;
					Debug.Log (touchPos.ToString()+" : "+crossPos.ToString());
					dx = (touchPos.x - crossPos.x) ;
					dy = (touchPos.y - crossPos.y) ;
				}
				else
				{
					Debug.Log (touchPos);
					dxj = touchPos.x - mousePos.x;
					dyj = touchPos.y - mousePos.y;
					dx = dxj; dy = dyj;
				}

				float adx = dx < 0 ? 0 - dx : dx;
				float ady = dy < 0 ? 0 - dy : dy;

				if (adx + ady > 75) 
				{
					#if UNITY_EDITOR
						Debug.Log (adx + ady);
					#endif
					distance = 45;
					mousePos = touchPos;
					if (adx > ady) 
					{
						if (dx > 0)
						{
							if (dxj >= 0)
								MoveRight ();
							else
								UnChangeKey (Nescafe.Controller.Button.Left);
						}
						else
						{
							if (dxj <= 0)
								MoveLeft ();
							else
								UnChangeKey (Nescafe.Controller.Button.Right);
						}
					} 
					else 
					{
						if (dy < 0)
						{
							if (dyj >= 0)
								MoveDown ();
							else
								UnChangeKey (Nescafe.Controller.Button.Up);
						}
						else
						{
							if (dyj >= 0)
								MoveUp ();
							else
								UnChangeKey (Nescafe.Controller.Button.Down);
						}
					}
				}
				//mousePos = Vector3.zero;
			}
			else 
			{
				mousePos = Vector2.zero;
				touchPos = Vector2.zero;
				ChangeKey (Nescafe.Controller.Button.No);
			}
		}
		else if (downscale>0)
		{
			downscale -= Time.deltaTime;
			transform.localScale = Vector3.one * downscale;
		}

	}

}
