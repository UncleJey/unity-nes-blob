using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nescafe;

public class TileHolder : MonoBehaviour 
{
	/// <summary>
	/// Текстуры
	/// </summary>
	[SerializeField]
	private Texture2D tiles;
	/// <summary>
	/// Собственный экземпляр
	/// </summary>
	static TileHolder instance;
	/// <summary>
	/// Консоль
	/// </summary>
	public static Console console;
	/// <summary>
	/// адреса спрайтов для быстрого поиска
	/// </summary>
	private List <string> address = new List<string> ();
	[HideInInspector]
	/// <summary>
	/// Спрайты
	/// </summary>
	public Sprite[] sprites;
	bool added = false;

	void Awake()
	{
		instance = this;

		address.Clear ();
		while (address.Count < 1024)
			address.Add ("");


		try
		{
			sprites = Resources.LoadAll<Sprite>(tiles.name);
			TextAsset ta = Resources.Load<TextAsset>(tiles.name+"_data");
			string vals = ta.text.Replace("\r",""); // System.IO.File.ReadAllText("Assets/Resources/" +tiles.name+"_data.txt").Replace("\r","");
			string[] data = vals.Split ("\n"[0]);
			address.Clear ();
			address.AddRange (data);

			while (address.Count > 1024)
				address.RemoveAt(1023);
		}
		catch
		{
		}
		while (address.Count < 1024)
			address.Add ("");
	}

	public static Sprite GetSprite(OAM pOAM)
	{
		return instance.findSprite (pOAM);
	}

#if UNITY_EDITOR

	void OnApplicationPauer(bool p)
	{
		if (p)
			OnDisable ();
	}

	void OnDisable()
	{
		if (!added)
			return;
		added = false;
		System.IO.File.WriteAllBytes("Assets/Resources/" + tiles.name+".png", tiles.EncodeToPNG());
		string vals = "";
		int cnt = address.Count;
		for (int i = 0; i < 1024; i++)
		{
			if (cnt > i)
				vals += (i==0?"" : "\n") + address [i];
			else
				vals += "\n";
		}
		System.IO.File.WriteAllText("Assets/Resources/" +tiles.name+"_data.txt", vals);
	}
#endif

	public static void ExChange(int p1, int p2)
	{
		instance.SwapSprites(p1, p2);
	}

	int firstFree(OAM pOAM)
	{
		if (pOAM.isBackGround)
		{
			for (int i = 640; i < 1024; i++)
			{
				if (string.IsNullOrEmpty (address [i]))
					return i;
			}
		}
		else
		{
			for (int i = 0; i < 640; i++)
			{
				if (string.IsNullOrEmpty (address [i]))
					return i;
			}
		}

		return 1023;
	}

	/// <summary>
	/// Поменять спрайты местами
	/// </summary>
	void SwapSprites(int p1, int p2)
	{
		added = true;
		Sprite s1 = sprites [p1];
		Sprite s2 = sprites [p2];
		Color c1,c2;
		int dim = (int)s1.rect.width;

		int x1 = (int) s1.rect.x; int y1 = (int) s1.rect.y;
		int x2 = (int) s2.rect.x; int y2 = (int) s2.rect.y;

		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{
				c1 = tiles.GetPixel (x1 + i, y1 + j);
				c2 = tiles.GetPixel (x2 + i, y2 + j);

				tiles.SetPixel (x1 + i, y1 + j, c2);
				tiles.SetPixel (x2 + i, y2 + j, c1);
			}
		}
		tiles.Apply();

		string n1 = address [p2]; 

		address [p2] = address [p1];
		address [p1] = n1;
	}

	/// <summary>
	/// Кеширование текстур
	/// </summary>
	public Sprite findSprite(OAM pOAM)
	{
		string k = pOAM.textureKey;
		int addr = address.IndexOf(k);
		if (addr >=0)
			return sprites [addr];

		addr = firstFree (pOAM);

		Sprite spr = sprites [addr];
		int xx = (int) spr.rect.x + 63 ;
		int yy = (int) spr.rect.y + 63 ;

		byte[,] spriteArray = console.Ppu.readSprite (pOAM);
		for (int i=0; i<64; i++)
		{
			for (int j=0; j<64; j++)
			{
				int ii = i >> 3;
				int jj = j >> 3;
				tiles.SetPixel (xx - i, yy - j, UNES.palette [spriteArray [ii, jj]]);
			}
		}
		added = true;
		tiles.Apply ();
		//UnityEditor.EditorUtility.SetDirty (tiles);
		Debug.Log("added "+addr);
		address[addr]=k;
		//sprites = Resources.LoadAll<Sprite>(tiles.name);
		return spr;
	}

}
