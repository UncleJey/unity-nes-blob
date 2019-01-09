using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesChanger : MonoBehaviour 
{
	public TileSprite prefab;
	static TileSprite selected;
	TileHolder holder;

	void Start()
	{
		holder = GetComponent<TileHolder> ();
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				TileSprite s = Instantiate (prefab);
				s.transform.SetParent (this.transform);
				s.transform.localScale = Vector3.one;
				s.transform.GetComponent<RectTransform>().anchoredPosition= new Vector2 (j * 64, 0-i * 64);
				s.spriteNum = j + i * 32;
				s.btn.image.sprite = holder.sprites [s.spriteNum];
				s.btn.image.SetNativeSize ();
			}
		}
	}

	public static void Select(TileSprite spr)
	{
		if (selected == null)
		{
			selected = spr;
			selected.toggler.SetActive (true);
		}
		else if (selected == spr)
		{
			selected = null;
			spr.toggler.SetActive (false);
		}
		else
		{
			selected.toggler.SetActive (false);
			TileHolder.ExChange (spr.spriteNum, selected.spriteNum);
			selected = null;
		}
	}
}
