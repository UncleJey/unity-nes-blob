using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fieldSprite : MonoBehaviour 
{
	/// <summary>
	/// Номер спрайта
	/// </summary>
	public int spriteNum = 1;
	/// <summary>
	/// Чётный / нечётный проход
	/// </summary>
	public bool EvenOdd;

	public MeshRenderer _renderer;
	public SpriteRenderer _sprite;

	/// <summary>
	/// Отображается ли спрайт в данный момент
	/// </summary>
	public bool active
	{
		get
		{
			return gameObject.activeSelf;
		}
		set
		{
			if (gameObject.activeSelf != value)
				gameObject.SetActive (value);
		}
	}

#if UNITY_EDITOR
	static void drawString(string text, Vector3 worldPos, Color? colour = null)
	{        
		UnityEditor.Handles.BeginGUI();
		if (colour.HasValue)
			GUI.color = colour.Value;
		var view = UnityEditor.SceneView.currentDrawingSceneView;
		Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

		if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
		{
			UnityEditor.Handles.EndGUI();
			return;
		}

		Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
		GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
		UnityEditor.Handles.EndGUI();
	}

	void OnDrawGizmos()
	{
		drawString(spriteNum.ToString(), transform.position, Color.white);
	}
#endif

	public void DrawSprite(Sprite pSprite)
	{
		_sprite.sprite = pSprite;
	}
}
