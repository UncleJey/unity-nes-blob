using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nescafe;
using System.IO;

public class UNES : MonoBehaviour 
{

	public Field field;
	public BackGround bg;

	static Console _console;
	public Texture2D _frame;
	public string fileName;

	public static Color32[] palette = new Color32[256];

	Dictionary <KeyCode, Nescafe.Controller.Button> keys = new Dictionary<KeyCode, Controller.Button>();
	bool running = false;

	public GameObject firstScreen;
	public GameObject controls;

	void Start()
	{
		_console = new Console();
		string fullpath;

		#if UNITY_EDITOR
			fullpath = Application.streamingAssetsPath + "/" + fileName;
		#elif UNITY_ANDROID
			fullpath = Application.persistentDataPath+"/"+fileName;

			if (!System.IO.File.Exists (fullpath))
			{
				WWW www = new WWW (Application.streamingAssetsPath + "/" + fileName);
				while (!www.isDone)	{}

				if (!string.IsNullOrEmpty (www.error))
					Debug.Log ("err " + www.error);
				else
					Debug.Log ("loaded " + www.bytes.Length + " bytes");

				Debug.Log ("save " + fullpath);
				System.IO.File.WriteAllBytes (fullpath, www.bytes);
			}
		#endif

		//_frame = new Texture2D(256, 240, TextureFormat.RGB24, false);
		InitKeys();
		InitPalette();

		LoadCartridge (fullpath);

		Application.targetFrameRate = 45;
	}

	void StartConsole()
	{
		//_thread = StartCoroutine (_console.thread ());
		field.console = _console;
		bg.console = _console;
		TileHolder.console = _console;
		running = true;
	}

	void StopConsole()
	{
		running = false;
	}

	void Draw(bool drawBG)
	{
		field.Draw ();
		if (_console.Ppu.changed || drawBG)  
		{
			_console.Ppu.ReadBGPattern ();
			bg.ReDraw (_console.Ppu.bg);

			if (bg.GetValue(ValTypes.isLevelSelect)>0)
			{
				if (!firstScreen.activeSelf)
				{
					firstScreen.gameObject.SetActive (true);
					bg.gameObject.SetActive (false);
				}
			}
			else
			{
				if (firstScreen.activeSelf)
				{
					firstScreen.gameObject.SetActive (false);
					bg.gameObject.SetActive (true);
				}
				bg.transform.localPosition = new Vector2 (0, field.scale * (_console.Ppu.FineY() - 7));
			}

			if (bg.GetValue(ValTypes.Level)>0)
				controls.gameObject.SetActive (true);
			else
				controls.gameObject.SetActive (false);
		}
	}

	void LoadCartridge(string pFileName)
	{
		StopConsole();
		if (_console.LoadCartridge (pFileName))
			StartConsole ();
		else
			UnityEngine.Debug.Log ("invalid cartrige");
	}

	void InitPalette()
	{
		palette[0x0] = new Color32(84, 84, 84, 0);
		palette[0x1] = new Color32(0, 30, 116, 255);
		palette[0x2] = new Color32(8, 16, 144, 255);
		palette[0x3] = new Color32(48, 0, 136, 255);
		palette[0x4] = new Color32(68, 0, 100, 255);
		palette[0x5] = new Color32(92, 0, 48, 255);
		palette[0x6] = new Color32(84, 4, 0, 255);
		palette[0x7] = new Color32(60, 24, 0, 255);
		palette[0x8] = new Color32(32, 42, 0, 255);
		palette[0x9] = new Color32(8, 58, 0, 255);
		palette[0xa] = new Color32(0, 64, 0, 255);
		palette[0xb] = new Color32(0, 60, 0, 255);
		palette[0xc] = new Color32(0, 50, 60, 255);
		palette[0xd] = new Color32(0, 0, 0, 255);
		palette[0xe] = new Color32(0, 0, 0, 255);
		palette[0xf] = new Color32(0, 0, 0, 255);
		palette[0x10] = new Color32(152, 150, 152, 255);
		palette[0x11] = new Color32(8, 76, 196, 255);
		palette[0x12] = new Color32(48, 50, 236, 255);
		palette[0x13] = new Color32(92, 30, 228, 255);
		palette[0x14] = new Color32(136, 20, 176, 255);
		palette[0x15] = new Color32(160, 20, 100, 255);
		palette[0x16] = new Color32(152, 34, 32, 255);
		palette[0x17] = new Color32(120, 60, 0, 255);
		palette[0x18] = new Color32(84, 90, 0, 255);
		palette[0x19] = new Color32(40, 114, 0, 255);
		palette[0x1a] = new Color32(8, 124, 0, 255);
		palette[0x1b] = new Color32(0, 118, 40, 255);
		palette[0x1c] = new Color32(0, 102, 120, 255);
		palette[0x1d] = new Color32(0, 0, 0, 255);
		palette[0x1e] = new Color32(0, 0, 0, 255);
		palette[0x1f] = new Color32(0, 0, 0, 255);
		palette[0x20] = new Color32(236, 238, 236, 255);
		palette[0x21] = new Color32(76, 154, 236, 255);
		palette[0x22] = new Color32(120, 124, 236, 255);
		palette[0x23] = new Color32(176, 98, 236, 255);
		palette[0x24] = new Color32(228, 84, 236, 255);
		palette[0x25] = new Color32(236, 88, 180, 255);
		palette[0x26] = new Color32(236, 106, 100, 255);
		palette[0x27] = new Color32(212, 136, 32, 255);
		palette[0x28] = new Color32(160, 170, 0, 255);
		palette[0x29] = new Color32(116, 196, 0, 255);
		palette[0x2a] = new Color32(76, 208, 32, 255);
		palette[0x2b] = new Color32(56, 204, 108, 255);
		palette[0x2c] = new Color32(56, 180, 204, 255);
		palette[0x2d] = new Color32(60, 60, 60, 255);
		palette[0x2e] = new Color32(0, 0, 0, 255);
		palette[0x2f] = new Color32(0, 0, 0, 255);
		palette[0x30] = new Color32(236, 238, 236, 255);
		palette[0x31] = new Color32(168, 204, 236, 255);
		palette[0x32] = new Color32(188, 188, 236, 255);
		palette[0x33] = new Color32(212, 178, 236, 255);
		palette[0x34] = new Color32(236, 174, 236, 255);
		palette[0x35] = new Color32(236, 174, 212, 255);
		palette[0x36] = new Color32(236, 180, 176, 255);
		palette[0x37] = new Color32(228, 196, 144, 255);
		palette[0x38] = new Color32(204, 210, 120, 255);
		palette[0x39] = new Color32(180, 222, 120, 255);
		palette[0x3a] = new Color32(168, 226, 144, 255);
		palette[0x3b] = new Color32(152, 226, 180, 255);
		palette[0x3c] = new Color32(160, 214, 228, 255);
		palette[0x3d] = new Color32(160, 162, 160, 255);
		palette[0x3e] = new Color32(0, 0, 0, 255);
		palette[0x3f] = new Color32(0, 0, 0, 255);
	}

	void InitKeys()
	{
		keys.Clear ();

		keys [KeyCode.UpArrow] = Controller.Button.Up;
		keys [KeyCode.DownArrow] = Controller.Button.Down;
		keys [KeyCode.LeftArrow] = Controller.Button.Left;
		keys [KeyCode.RightArrow] = Controller.Button.Right;

		keys [KeyCode.Q] = Controller.Button.Select;
		keys [KeyCode.W] = Controller.Button.Start;

		keys [KeyCode.A] = Controller.Button.A;
		keys [KeyCode.S] = Controller.Button.B;
	}

	public static void PressButton(Controller.Button pButton, bool pPressed)
	{
		_console.Controller.setButtonState (pButton, pPressed);
	}

	float bgTimer;
	void Update()
	{
		foreach (KeyValuePair<KeyCode, Controller.Button> k in keys)
		{
			if (Input.GetKeyDown (k.Key))
				_console.Controller.setButtonState (k.Value, true);
			else if (Input.GetKeyUp (k.Key))
				_console.Controller.setButtonState (k.Value, false);
		}

		bgTimer -= Time.deltaTime;

		if (running) 
		{
			_console.GoUntilFrame ();
			Draw (bgTimer < 0);
			if (bgTimer < 0)
				bgTimer = 0.5f;

		}
	}
}
