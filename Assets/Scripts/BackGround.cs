using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nescafe;

/// <summary>
/// Типы значений
/// </summary>
public enum ValTypes : byte
{
	 None = 0
	/// <summary>
	/// Номер уровня
	/// </summary>
	,Level = 1
	/// <summary>
	/// Очки
	/// </summary>
	,Score = 2
	/// <summary>
	/// Количество жизней
	/// </summary>
	,Live = 3
	/// <summary>
	/// Бонус за прохождение
	/// </summary>
	,Bonus = 4
	/// <summary>
	/// Специальный бонусный уровень
	/// </summary>
	,SpecialBonus = 5
	/// <summary>
	/// Рекорд
	/// </summary>
	,TopScore = 6
	/// <summary>
	/// Это простой уровень
	/// </summary>
	,isSimpleLevel = 7
	/// <summary>
	/// Выбор уровня
	/// </summary>
	,isLevelSelect = 8
}

[System.Serializable]
public class Grabber
{
	/// <summary>
	/// Имя граббера
	/// </summary>
	public string name;
	/// <summary>
	/// Тип значений
	/// </summary>
	public ValTypes type;
	/// <summary>
	/// Номер строки со значением
	/// </summary>
	public int rowNum;
	/// <summary>
	/// Позиции ячеек значений
	/// </summary>
	public int[] cells;
	/// <summary>
	/// Текущее значение
	/// </summary>
	public int value;
}

public class BackGround : MonoBehaviour 
{
	public Console console;
	public fieldSprite defPrefab;
	public Field field;

	fieldSprite[,] cells;

	/// <summary>
	/// Грабберы значений
	/// </summary>
	[SerializeField]
	Grabber[] grabbers;
	/// <summary>
	/// Номера спрайтов цифр
	/// </summary>
	[SerializeField]
	byte[] digits;
	// 0 - 48 1 - 49 2 - 50  3  4 - 52

	public static readonly float scale = 6.2f;
	public static readonly int NotSetted = -10;

	public void ReDraw(OAM[,] pOAM)
	{
//		string ss = "";
		if (cells == null) 
			cells = new fieldSprite[console.Ppu.bgSizeX, console.Ppu.bgSizeY];
		for (int i = 0; i < console.Ppu.bgSizeX; i++) 
		{
			for (int j = 0; j < console.Ppu.bgSizeY; j++) 
			{
				if (cells [i, j] == null) 
				{
					cells [i, j] = Instantiate(defPrefab) as fieldSprite;
					cells [i, j].transform.parent = transform;
					cells [i, j].transform.localScale = Vector3.one * 10;
					cells [i, j].name = "c_" + i + "_" + j;
				}

				cells [i, j].transform.localPosition = new Vector3 (scale * i ,  0 - scale * j , 0 );
				cells [i, j].spriteNum = pOAM[i,j].spriteNum;
				//cells [i, j].EvenOdd = pEvenOdd;
				if (console.Ppu.flagShowBackground != 0)
				{
					cells [i, j].active = pOAM [i, j].active;
					cells [i, j].DrawSprite (TileHolder.GetSprite (pOAM [i, j]));
				}
				else 
				{
					cells [i, j].active = false;
				}
//					ss += pOAM [i, j].spriteNum.ToString ("x2") + " ";
			}
//				ss += "\r\n";
		}
		Grab ();
//		Debug.Log (ss);
	}

	/// <summary>
	/// Номер спрайта в цифру
	/// </summary>
	string SpriteToNum(int pSprite)
	{
		byte sn = (byte)pSprite;
		for (int i = digits.Length - 1; i >= 0; i--)
		{
			if (digits [i] == sn)
				return i.ToString ();
		}
		return "";
	}

	void Grab()
	{
		foreach (Grabber g in grabbers)
		{
			string v = "";
			foreach (int xx in g.cells)
			{
				v += SpriteToNum (cells [xx, g.rowNum].spriteNum);
			}

			if (string.IsNullOrEmpty(v) || !int.TryParse (v, out g.value))
				g.value = NotSetted;
		}
	}

	/// <summary>
	/// Получить значение
	/// </summary>
	public int GetValue(ValTypes pType)
	{
		for (int i = grabbers.Length - 1; i >= 0; i--)
		{
			if (grabbers [i].type == pType)
				return grabbers [i].value;
		}
		return NotSetted;
	}
}
