using System;
using UnityEngine;

namespace Nescafe
{
	/// <summary>
	/// Информация о спрайтах выводимых на экран
	/// </summary>
	public struct OAM
	{
		/// <summary>
		/// 0 - Абсолютная координата верхнего левого угла спрайта по вертикали.
		/// </summary>
		public byte yTop;
		/// <summary>
		/// 1 - Номер иконки из знакогенератора
		/// </summary>
		public byte spriteNum;

		public bool active
		{
			get
			{
				if (yTop == xTop && yTop == spriteNum && yTop == 248)
					return false;
				return true;
			}
		}

		private byte _spriteAttr;
		/// <summary>
		/// Атрибуты спрайта
		/// </summary>
		public byte spriteAttr
		{
			set 
			{
				_spriteAttr = value;
				paletteNum = (byte)(_spriteAttr & 0x03);
				flipHoriz  = (_spriteAttr & 0x40)  != 0;
				flipVert   = (_spriteAttr & 0x80)  != 0;
				priority   = (_spriteAttr & 0x20)  != 0;
			}
			get 
			{
				return _spriteAttr;
			}

		}
		/// <summary>
		/// Координата верхнего левого угла спрайта по горизонтали.
		/// </summary>
		public byte xTop;
		/// <summary>
		/// Отразить по горизонтали
		/// </summary>
		public bool flipHoriz;
		/// <summary>
		/// Отразить по вертикали
		/// </summary>
		public bool flipVert;
		/// <summary>
		/// номер палитры
		/// </summary>
		public byte paletteNum;
		/// <summary>
		/// Спрайт принадлежит фону
		/// </summary>
		public bool isBackGround;
		/// <summary>
		/// Приоритет отрисовки
		/// false - background
		/// </summary>
		public bool priority;
		/// <summary>
		/// Уникальный ключ текстуры
		/// </summary>
		public string textureKey
		{
			get
			{
				return (isBackGround?"b":"f")+spriteNum + "_" + paletteNum;
			}
		}
		/// <summary>
		/// Сброс
		/// </summary>
		public void Clear()
		{
			yTop = 0;
			spriteNum = 0;
			_spriteAttr = 0;
			paletteNum = 0;
			flipHoriz = false;
			flipVert = false;
			priority = false;
			xTop = 0;
		}
		/*
		/// <summary>
		/// значение
		/// </summary>
		public byte[] val
		{
			set 
			{
				yTop = value [0];
				spriteNum = value [1];
				spriteAttr = value [2];
				xTop = value [3];
			}
			get 
			{
				return new byte[4] {yTop, spriteNum, _spriteAttr, xTop};
			}
		}
		*/
		/// <summary>
		/// записать данные по смещению
		/// младшие 2 бита адреса - это номер поля
		/// </summary>
		public void Write(ushort addr, byte data)
		{
			switch (addr & 3) 
			{
				case 0:
					yTop = data;
				break;
				case 1:
					spriteNum = data;
				break;
				case 2:
					spriteAttr = data;
				break;
				case 3:
					xTop = data;
				break;
			}
		}
		/// <summary>
		/// прочитать данные по смещению
		/// младшие 2 бита адреса - это номер поля
		/// </summary>
		public byte Read(ushort addr)
		{
			byte val = 0;
			switch (addr & 3) 
			{
				case 0:
					val = yTop;
				break;
				case 1:
					val = spriteNum ;
				break;
				case 2:
					val = _spriteAttr;
				break;
				case 3:
					val = xTop;
				break;
			}
			return val;
		}
	}


    /// <summary>
    /// Represents a NTSC
	/// 
	/// [P]icture 
	/// [P]rocessor
	/// [U]nit
	/// 
	/// http://dendy.migera.ru/nes/g02.html
	/// https://geektimes.ru/post/258028/
	/// 
    /// </summary>
    public class Ppu
    {
		/// <summary>
		/// Данные изменены, нужно перерисовать
		/// </summary>
		public bool changed = false;

        readonly PpuMemory _memory;
        readonly Console _console;
		static readonly byte oamLength = 64;
		static readonly byte sprLength = 8;

        // OAM / Sprite rendering
        //byte[] _oam;

		/// <summary>
		/// Информация о спрайтах и их позициях
		/// может содержать информацию о 64 спрайтах (по 4 байт)
		/// </summary>
		public OAM[] _OAM;

        ushort _oamAddr;
		/// <summary>
		/// The sprites.
		/// </summary>
		public OAM[] _Sprites;

        int _numSprites;

        /// <summary>
        /// Gets the current scanline number.
        /// </summary>
        /// <value>The current scanline number.</value>
        public int Scanline { get; private set; }

        /// <summary>
        /// Gets the cycle number on the current scanline.
        /// </summary>
        /// <value>The cycle number on the current scanline.</value>
        public int Cycle { get; private set; }

        // Base background nametable address
        //ushort _baseNametableAddress;

        // Address of pattern table used for background
		/// <summary>
		/// Адрес активной экранной страницы ($2000; $2400; $2800; $2C00)
		/// </summary>
        ushort _bgPatternTableAddress;

        // Base sprite pattern table address
        ushort _spritePatternTableAddress;

        // Vram increment per write to PPUDATA
        int _vRamIncrement;

        // Last value written to a PPU register
        byte _lastRegisterWrite;

        // PPUCTRL Register flags
		/// <summary>
		/// Адрес страницы фона
		/// </summary>
		byte bgPageAddress;
		/*
		(0,0)     (256,0)     (511,0)
		+-----------+-----------+
		|      0    |     1     |
		|           |           |
		|   $2000   |   $2400   |
		|           |           |
		|           |           |
 (0,240)+-----------+-----------+(511,240)
		|     2     |    3      |
		|           |           |
		|   $2800   |   $2C00   |
		|           |           |
		|           |           |
		+-----------+-----------+
		(0,479)   (256,479)   (511,479)
		*/

        //byte _flagBaseNametableAddr;
		/// <summary>
		/// Выбор режима инкремента адреса при обращении к видеопамяти (0 – увеличение на единицу «горизонтальная запись»; 1 - увеличение на 32 «вертикальная запись»)
		/// </summary>
        byte _flagVRamIncrement;
		/// <summary>
		/// Выбор знакогенератора спрайтов (0/1)
		/// </summary>
        byte _flagSpritePatternTableAddr;
		/// <summary>
		/// Выбор знакогенератора фона (0/1)
		/// </summary>
        byte _flagBgPatternTableAddr;
		/// <summary>
		///  Размер спрайтов (0 - 8x8; 1 - 8x16)
		/// </summary>
        byte _flagSpriteSize;
        //byte _flagMasterSlaveSelect;
		/// <summary>
		/// Формирование запроса прерывания NMI при кадровом синхроимпульсе	(0 - запрещено; 1 - разрешено)
		/// </summary>
        byte _nmiOutput;

        // NMI Occurred flag
        byte _nmiOccurred;

        // PPUMASK Register flags
        //byte _flagGreyscale;

		/// <summary>
		/// 0 – Рисунок фона невиден в крайнем левом столбце; 1- Весь фон виден
		/// </summary>
        //byte _flagShowBackgroundLeft;

		/// <summary>
		/// 0 – Спрайты не видны в крайнем левом столбце; 1- Все спрайты видны
		/// </summary>
        byte _flagShowSpritesLeft;
		/// <summary>
		/// 0 – Фон не отображается; 1 – Фон отображается
		/// </summary>
        public byte flagShowBackground;
		/// <summary>
		/// 0 – Спрайты не отображаются; 1 – Спрайты отображаются
		/// </summary>
		public byte flagShowSprites;

		//Яркость экрана/интенсивность цвета в RGB (в Денди не используется)
        //byte _flagEmphasizeRed;
        //byte _flagEmphasizeGreen;
        //byte _flagEmphasizeBlue;

        // Internal PPU Registers
        ushort v; // Current VRAM address (15 bits)
		/// <summary>
		/// Адрес активной экранной страницы (00 – $2000; 01 – $2400; 10 – $2800; 11 - $2C00)
		/// </summary>
		ushort t; // Temporary VRAM address (15 bits) 
        byte x; // Fine X scroll (3 bits)
        byte w; // First or second write toggle (1 bit)
        byte f; // Even odd flag (even = 0, odd = 1)

		byte hScroll;
		byte vScroll = 0;

        // Tile shift register and variables (latches) that feed it every 8 cycles
        ulong _tileShiftReg;
        byte _nameTableByte;
        byte _attributeTableByte;
        byte _tileBitfieldLo;
        byte _tileBitfieldHi;

        // PPUDATA buffer
        byte _ppuDataBuffer;

        /// <summary>
        /// Is <c>true</c> if rendering is currently enabled.
        /// </summary>
        /// <value><c>true</c> if rendering is enabled; otherwise, <c>false</c>.</value>
        public bool RenderingEnabled
        {
            get { return flagShowSprites != 0 || flagShowBackground != 0; }
        }

        /// <summary>
        /// Constructs a new PPU.
        /// </summary>
        /// <param name="console">Console that this PPU is a part of</param>
        public Ppu(Console console)
        {
            _memory = console.PpuMemory;
            _console = console;

            //BitmapData = new byte[256 * 240];

            //_oam = new byte[256];
			_OAM = new OAM[oamLength];

			//_sprites = new byte[32];
			_Sprites = new OAM[sprLength];

			Reset ();
        }

        /// <summary>
        /// Resets this PPU to its startup state.
        /// </summary>
        public void Reset()
        {
            //Array.Clear(BitmapData, 0, BitmapData.Length);

            Scanline = 240;
            Cycle = 340;

            _nmiOccurred = 0;
            _nmiOutput = 0;

            w = 0;
            f = 0;

			for (int i = 0; i<oamLength; i++)
				_OAM [i].Clear ();

			for (int i = 0; i<sprLength; i++)
				_Sprites [i].Clear ();

			for (int i = 0; i < bgSizeY; i++) 
			{
				for (int j = 0; j < bgSizeX; j++) 
				{
					bg [j, i].xTop = (byte)j;
					bg [j, i].yTop = (byte)i;
					bg [j, i].spriteNum = 0;
					bg [j, i].isBackGround = true;
					bg [j, i].spriteAttr = 0;
				}
			}
     	}

        byte LookupBackgroundColor(byte data)
        {
            int colorNum = data & 0x3;
            int paletteNum = (data >> 2) & 0x3;

            // Special case for universal background color
            if (colorNum == 0) 
				return _memory.Read(0x3F00);

            ushort paletteAddress;
            switch (paletteNum)
            {
                case 0:
                    paletteAddress = (ushort)0x3F01;
                    break;
                case 1:
                    paletteAddress = (ushort)0x3F05;
                    break;
                case 2:
                    paletteAddress = (ushort)0x3F09;
                    break;
                case 3:
                    paletteAddress = (ushort)0x3F0D;
                    break;
                default:
                    throw new Exception("Invalid background palette Number: " + paletteNum.ToString());
            }

            paletteAddress += (ushort)(colorNum - 1);
            return _memory.Read(paletteAddress);
        }

        byte LookupSpriteColor(byte data)
        {
            int colorNum = data & 0x3;
            int paletteNum = (data >> 2) & 0x3;

            // Special case for universal background color
            if (colorNum == 0) 
				return _memory.Read(0x3F00);

            ushort paletteAddress;
            switch (paletteNum)
            {
                case 0:
                    paletteAddress = (ushort)0x3F11;
                    break;
                case 1:
                    paletteAddress = (ushort)0x3F15;
                    break;
                case 2:
                    paletteAddress = (ushort)0x3F19;
                    break;
                case 3:
                    paletteAddress = (ushort)0x3F1D;
                    break;
                default:
                    throw new Exception("Invalid background palette Number: " + paletteNum.ToString());
            }

            paletteAddress += (ushort)(colorNum - 1);
            return _memory.Read(paletteAddress);
        }

		public byte bgSizeX = 32; 
		public byte bgSizeY = 30;

		public OAM[,] bg = new OAM[32, 30];
		/// <summary>
		/// Хак
		/// Нужно перерисовать фон
		/// </summary>
		public void ReadBGPattern()
		{
			ushort address;
			changed = false;
			switch (bgPageAddress)  // адрес выбранной страницы фона
			{
				case 1:
					address = 0x2400;
				break;
				case 2:
					address = 0x2800;
				break;
				case 3:
					address = 0x2C00;
				break;
				default: // 0
					address = 0x2000;
				break;
			}

			for (int i = 0; i < bgSizeY; i++) 
			{
				for (int j = 0; j < bgSizeX; j++) 
				{
					byte sn = _memory.Read ((ushort)(address + (i << 5) + j));
					if (bg [j, i].spriteNum != sn) 
					{
						bg [j, i].spriteNum = sn;

						// сложение криво выполняется, поэтому каждую переменную определяем отдельно
						ushort addr = (ushort)(address + 0x3C0);
						ushort ii = (ushort)((i >> 2) << 3);
						ushort jj = (ushort)(j >> 2);

						addr += (ushort)(ii + jj);
						byte dj = (byte)((j & 2));
						byte di = (byte)((i & 2) << 1);
						//					byte delta = (byte)(3 << (dj + di));
						//string de = addr.ToString ("x4") + ":"+delta.ToString("D");

						byte attr = _memory.Read (addr); 

						bg [j, i].spriteAttr = (byte)((attr >> (di + dj)) & 3);
					}
				}
			}
		}

		/// <summary>
		/// Спрайт
		/// </summary>
		public byte[,] readSprite(OAM pOAM)
		{
			byte xx = 8, yy = 8;
			// 8x8 or 8x16
			ushort _currSpritePatternTableAddr = pOAM.isBackGround?_bgPatternTableAddress: _spritePatternTableAddress;

			byte patternIndex;

			if (_flagSpriteSize == 1)
			{
				_currSpritePatternTableAddr = (ushort)((pOAM.spriteNum & 1) * 0x1000);
				patternIndex = (byte)(pOAM.spriteNum & 0xFE);
				yy = 16;
			}
			else
			{
				patternIndex = (byte)(pOAM.spriteNum);
			}

			ushort patternAddress = (ushort)(_currSpritePatternTableAddr + (patternIndex * 16));
				
			byte[,] spriteArray = new byte[xx, yy];

			Array.Clear (spriteArray, 0, xx * yy);
			for (int i = 0; i < xx; i++)
			{
				for (int j = 0; j < yy; j++)
				{
					byte colorNum = (byte) GetSpritePatternPixel
						(
							patternAddress
							,i
							,j
							,true //pOAM.flipHoriz
							,false //pOAM.flipVert
						);

					byte paletteNum = pOAM.paletteNum;
					colorNum = (byte)(((paletteNum << 2) | colorNum) & 0xF);

					if ((colorNum & 3) == 0)
					{
						spriteArray [i, j] = 0;
					}
					else
					{
						colorNum = pOAM.isBackGround?
							LookupBackgroundColor((byte)( colorNum)):
							LookupSpriteColor((byte)( colorNum));
						spriteArray [i, j] = colorNum;
					}
				}
			}
			return spriteArray;
		}

        void CopyHorizPositionData()
        {
            // v: ....F.. ...EDCBA = t: ....F.. ...EDCBA
            v = (ushort)((v & 0x7BE0) | (t & 0x041F));
        }

        void CopyVertPositionData()
        {
            // v: IHGF.ED CBA..... = t: IHGF.ED CBA.....
            v = (ushort)((v & 0x041F) | (t & 0x7BE0));
        }

        int CoarseX()
        {
            return v & 0x1f;
        }

        int CoarseY()
        {
            return (v >> 5) & 0x1f;
        }

        public int FineY()
        {
			return vScroll & 7;
        }

        int GetSpritePatternPixel(ushort patternAddr, int xPos, int yPos, bool flipHoriz = false, bool flipVert = false)
        {
            int h = _flagSpriteSize == 0 ? 7 : 15;

            // Flip x and y if needed
            xPos = flipHoriz ? 7 - xPos : xPos;
            yPos = flipVert ? h - yPos : yPos;

            // First byte in bitfield, wrapping accordingly for y > 7 (8x16 sprites)
            ushort yAddr;
            if (yPos <= 7)
				yAddr = (ushort)(patternAddr + yPos);
            else
				yAddr = (ushort)(patternAddr + 16 + (yPos - 8)); // Go to next tile for 8x16 sprites

            // Read the 2 bytes in the bitfield for the y coordinate
            byte[] pattern = new byte[2];
            pattern[0] = _memory.Read(yAddr);
            pattern[1] = _memory.Read((ushort)(yAddr + 8));

            // Extract correct bits based on x coordinate
            byte loBit = (byte)((pattern[0] >> (7 - xPos)) & 1);
            byte hiBit = (byte)((pattern[1] >> (7 - xPos)) & 1);

            return ((hiBit << 1) | loBit) & 0x03;
        }

        void IncrementX()
        {
            if ((v & 0x001F) == 31)
            {
                v = (ushort)(v & (~0x001F)); // Reset Coarse X
                v = (ushort)(v ^ 0x0400); // Switch horizontal nametable
            }
            else
            {
                v++; // Increment Coarse X
            }
        }

        void IncrementY()
        {
            if ((v & 0x7000) != 0x7000)
            { // if fine Y < 7
                v += 0x1000; // increment fine Y
            }
            else
            {
                v = (ushort)(v & ~0x7000); // Set fine Y to 0
                int y = (v & 0x03E0) >> 5; // y = coarse Y
                if (y == 29)
                {
                    y = 0; // coarse Y = 0
                    v = (ushort)(v ^ 0x0800); // switch vertical nametable
                }
                else if (y == 31)
                {
                    y = 0; // coarse Y = 0, nametable not switched
                }
                else
                {
                    y += 1; // Increment coarse Y
                }
                v = (ushort)((v & ~0x03E0) | (y << 5)); // Put coarse Y back into v
            }
        }

        /// <summary>
        /// Executes a single PPU step.
        /// </summary>
        public void Step()
        {
			// Trigger an NMI at the start of _scanline 241 if VBLANK NMI's are enabled
			if (Scanline == 241)// && Cycle == 1)
			{
				_nmiOccurred = 1;
				if (_nmiOutput != 0)
					_console.Cpu.TriggerNmi();
				Scanline = 242;
				//Cycle = 0;
			}
			Cycle += 8;
			//Cycle++; 

			// Reset cycle (and scanline if scanline == 260)
			// Also set to next frame if at end of last _scanline
			if (Cycle > 340)
			{
				if (Scanline >= 261) // Last scanline, reset to upper left corner
				{
					f ^= 1;
					Scanline = 0;
					Cycle = -1;
					_console.DrawFrame();
				}
				else // Not on last scanline
				{
					Cycle = -1;
					Scanline++;
				}
			}
        }

        /// <summary>
        /// Reads a byte from the register at the specified address.
        /// </summary>
        /// <returns>A byte read from the register at the specified address</returns>
        /// <param name="address">The address of the register to read from</param>
        public byte ReadFromRegister(ushort address)
        {
            byte data;
            switch (address)
            {
				case 0x2002: //Состояние видеопроцессора. Чтение сбрасывает некоторые биты!
                    data = ReadPpuStatus();
                break;
                case 0x2004:
                    data = ReadOamData();
                    break;
                case 0x2007:
                    data = ReadPpuData();
                    break;
                default:
                    throw new Exception("Invalid PPU Register read from register: " + address.ToString("X4"));
            }

            return data;
        }

        /// <summary>
        /// Writes a byte to the register at the specified address.
        /// </summary>
        /// <param name="address">The address of the register to write to</param>
        /// <param name="data">The byte to write to the register</param>
        public void WriteToRegister(ushort address, byte data)
        {
            _lastRegisterWrite = data;
            switch (address)
            {
				case 0x2000: // Управление видеопроцессором
					WritePpuCtrl (data);
					changed = true;
                break;
				case 0x2001: // Управление видеопроцессором
                    WritePpuMask(data);
					changed = true;
                break;
                case 0x2003:
                    WriteOamAddr(data);
                break;
                case 0x2004:
                    WriteOamData(data);
                break;
                case 0x2005:
                    WritePpuScroll(data);
                break;
                case 0x2006:
                    WritePpuAddr(data);
                break;
                case 0x2007:
                    WritePpuData(data);
                break;
                case 0x4014:
                    WriteOamDma(data);
                break;
                default:
                    throw new Exception("Invalid PPU Register write to register: " + address.ToString("X4"));
            }
        }

		/// <summary>
		/// Управление видеопроцессором
		/// $2000
		/// </summary>
        void WritePpuCtrl(byte data)
        {
            //_flagBaseNametableAddr = (byte)(data & 0x3);
			//Выбор режима инкремента адреса при обращении к видеопамяти (0 – увеличение на единицу «горизонтальная запись»; 1 - увеличение на 32 «вертикальная запись»)
            _flagVRamIncrement = (byte)((data >> 2) & 1); 
			//Выбор знакогенератора спрайтов (0/1)
            _flagSpritePatternTableAddr = (byte)((data >> 3) & 1);
			//Выбор знакогенератора фона (0/1)
            _flagBgPatternTableAddr = (byte)((data >> 4) & 1);
			// Размер спрайтов (0 - 8x8; 1 - 8x16)
            _flagSpriteSize = (byte)((data >> 5) & 1);
            //_flagMasterSlaveSelect = (byte)((data >> 6) & 1);
			//Формирование запроса прерывания NMI при кадровом синхроимпульсе (0 - запрещено; 1 - разрешено)
            _nmiOutput = (byte)((data >> 7) & 1);

            // Set values based off flags
            //_baseNametableAddress = (ushort)(0x2000 + 0x400 * _flagBaseNametableAddr);
            _vRamIncrement = (_flagVRamIncrement == 0) ? 1 : 32;
            _bgPatternTableAddress = (ushort)(_flagBgPatternTableAddr == 0 ? 0x0000 : 0x1000);
            _spritePatternTableAddress = (ushort)(0x1000 * _flagSpritePatternTableAddr);

            // t: ...BA.. ........ = d: ......BA
			//Адрес активной экранной страницы (00 – $2000; 01 – $2400; 10 – $2800; 11 - $2C00)
			bgPageAddress = (byte) (data & 0x03);

            t = (ushort)((t & 0xF3FF) | ((data & 0x03) << 10));

//			Debug.LogFormat ("write ppu ctl\n spritePat:{0}\nbgPat:{1}\nspriteSize:{2}\nbgPage:{3}", _flagSpritePatternTableAddr, _flagBgPatternTableAddr, _flagSpriteSize, bgPageAddress);
//			ReadBGPattern ();
        }

		/// <summary>
		/// Управление видеопроцессором
		/// $2001
		/// </summary>
        void WritePpuMask(byte data)
        {
			//Debug.Log ("wite PPU Mask " + ToBit (data));
            //_flagGreyscale = (byte)(data & 1);

			// 0 – Рисунок фона невиден в крайнем левом столбце; 1- Весь фон виден
            //_flagShowBackgroundLeft = (byte)((data >> 1) & 1); 

			// 0 – Спрайты невидны в крайнем левом столбце; 1- Все спрайты видны
            //_flagShowSpritesLeft = (byte)((data >> 2) & 1);

			// 0 – Фон не отображается; 1 – Фон отображается
            flagShowBackground = (byte)((data >> 3) & 1);

			//0 – Спрайты не отображаются; 1 – Спрайты отображаются
            flagShowSprites = (byte)((data >> 4) & 1);

            //_flagEmphasizeRed = (byte)((data >> 5) & 1);
            //_flagEmphasizeGreen = (byte)((data >> 6) & 1);
            //_flagEmphasizeBlue = (byte)((data >> 7) & 1);
        }

        // $4014
        void WriteOamAddr(byte data)
        {
            _oamAddr = data;
        }

        // $2004
        void WriteOamData(byte data)
        {
			_OAM[_oamAddr>>2].Write(_oamAddr, data);
            _oamAddr++;
        }

        // $2005
        void WritePpuScroll(byte data)
        {
            if (w == 0) // First write
            {
                // t: ....... ...HGFED = d: HGFED...
                // x:              CBA = d: .....CBA
                // w:                  = 1
                //t = (ushort)((t & 0xFFE0) | (data >> 3));
                //x = (byte)(data & 0x07);
				//hScroll = data;
                w = 1;
            }
            else
            {
                // t: CBA..HG FED..... = d: HGFEDCBA
                // w:                  = 0
				//if (t == 8302)
				if (vScroll == 0 || data < 255 || vScroll == 254) 
					vScroll = data;
				/*
				t = (ushort)(t & 0xC1F);
                t |= (ushort)((data & 0x07) << 12); // CBA
                t |= (ushort)((data & 0xF8) << 2); // HG FED
                */
                w = 0;
            }
        }

        // $2006
        void WritePpuAddr(byte data)
        {
            if (w == 0)  // First write
            {
                // t: .FEDCBA ........ = d: ..FEDCBA
                // t: X...... ........ = 0
                // w:                  = 1
                t = (ushort)((t & 0x00FF) | (data << 8));
                w = 1;
            }
            else
            {
                // t: ....... HGFEDCBA = d: HGFEDCBA
                // v                   = t
                // w:                  = 0
                t = (ushort)((t & 0xFF00) | data);
                v = t;
                w = 0;
            }
        }

        // $2007
        void WritePpuData(byte data)
        {
            _memory.Write(v, data);
            v += (ushort)(_vRamIncrement);
        }

        // $4014
        void WriteOamDma(byte data)
        {
            ushort startAddr = (ushort)(data << 8);

			byte index = (byte)( _oamAddr >> 2);
			_console.CpuMemory.ReadBufOAM(_OAM, index, startAddr, 255);

            // OAM DMA always takes at least 513 CPU cycles
            _console.Cpu.AddIdleCycles(513);

            // OAM DMA takes an extra CPU cycle if executed on an odd CPU cycle
            if (_console.Cpu.Cycles % 2 == 1) 
				_console.Cpu.AddIdleCycles(1);
        }

		/// <summary>
		/// Состояние видеопроцессора. Чтение сбрасывает некоторые биты!
		/// $2002
		/// </summary>
        byte ReadPpuStatus()
        {
            byte retVal = 0;
            retVal |= (byte)(_lastRegisterWrite & 0x1F); // Least signifigant 5 bits of last register write
            //retVal |= (byte)(_flagSpriteOverflow << 5);
            //retVal |= (byte)(_flagSpriteZeroHit << 6);
            retVal |= (byte)(_nmiOccurred << 7);

            // Old status of _nmiOccurred is returned then _nmiOccurred is cleared
            _nmiOccurred = 0;

            // w:                  = 0
            w = 0;

            return retVal;
        }

        // $2004
        byte ReadOamData()
        {
			return _OAM[_oamAddr>>2].Read(_oamAddr);
        }

        // $2007
        byte ReadPpuData()
        {
            byte data = _memory.Read(v);

            // Buffered read emulation
            // https://wiki.nesdev.com/w/index.php/PPU_registers#The_PPUDATA_read_buffer_.28post-fetch.29
            if (v < 0x3F00)
            {
                byte bufferedData = _ppuDataBuffer;
                _ppuDataBuffer = data;
                data = bufferedData;
            }
            else
            {
                _ppuDataBuffer = _memory.Read((ushort) (v - 0x1000));
            }

            v += (ushort)(_vRamIncrement);
            return data;
        }
    }
}
