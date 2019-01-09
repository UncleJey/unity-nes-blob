using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSprite : MonoBehaviour 
{
	public Button btn;
	public string spriteName;
	public int spriteNum;
	public GameObject toggler;

	void Awake()
	{
		btn.onClick.AddListener (()=>{TilesChanger.Select(this);});
	}
}
