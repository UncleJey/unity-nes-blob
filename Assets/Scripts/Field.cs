using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nescafe;

public class Field : MonoBehaviour 
{
	public fieldSprite defaultSprite;

	List <fieldSprite> sprites = new List<fieldSprite>();

	public Console console;
	fieldSprite[,] background;
	public bool levelSelect = false;
	[SerializeField]
	int LevelSelectSpriteNum;

	public float scale = 0;
	void Awake()
	{
		scale = BackGround.scale / 8f;
	}

	/// <summary>
	/// Отрисовка поля
	/// </summary>
	public void Draw()
	{
		// чётный / нечётный кадр
		bool frameEvenOdd = console._frameEvenOdd;
		levelSelect = false;

		if (console.Ppu.RenderingEnabled)
		{
			foreach (OAM o in console.Ppu._OAM)
			{
				if (o.active && console.Ppu.flagShowSprites != 0)
				{
					DrawSprite (frameEvenOdd, o);
					if (o.spriteNum == LevelSelectSpriteNum)
						levelSelect = true;
				}
			}
		}

		List<fieldSprite> sprs = sprites.FindAll (s => s.EvenOdd != frameEvenOdd && s.active);
		for (int i = sprs.Count - 1; i >= 0; i--)
			sprs [i].active = false;
	}

	fieldSprite DrawSprite(bool pEvenOdd, OAM pOAM)
	{
		// сначала пытаемся найти тот же спрайт
		fieldSprite spr = sprites.Find (s => /*s.spriteNum == pOAM.spriteNum &&*/ s.EvenOdd != pEvenOdd && s.active);

		// пытаемся найти неактивный спрайт
		if (spr == null)
			spr = sprites.Find (s => !s.active);
		
		// клонируем
		if (spr == null)
		{
			spr = Instantiate(defaultSprite) as fieldSprite;
			spr.transform.SetParent(transform);
			spr.transform.localScale = Vector3.one * 10;
			sprites.Add (spr);
		}

		spr.transform.localPosition =  new Vector3 (0f + scale * pOAM.xTop,  0f - scale * pOAM.yTop, 0 );
		spr.spriteNum = pOAM.spriteNum;
		spr.EvenOdd = pEvenOdd;
		spr.active = true;
		pOAM.isBackGround = false;
		spr.DrawSprite (TileHolder.GetSprite(pOAM));
		return spr;
	}

}
