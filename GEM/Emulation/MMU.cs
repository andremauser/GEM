using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GEM.Emulation
{
    public class MMU
    {

        #region Fields

        byte[] _bootROM;
        byte[] _videoRAM;
        byte[] _workRAM;
        byte[] _oamRAM;
        byte[] _highRAM;
        // Timer
        int _divCycleCount;
        int _timaCycleCount;

        #endregion

        #region Constructors

        public MMU()
        {
            _bootROM = new byte[] { 0x31, 0xFE, 0xFF, 0xAF, 0x21, 0xFF, 0x9F, 0x32, 0xCB, 0x7C, 0x20, 0xFB, 0x21, 0x26, 0xFF, 0x0E,
                                    0x11, 0x3E, 0x80, 0x32, 0xE2, 0x0C, 0x3E, 0xF3, 0xE2, 0x32, 0x3E, 0x77, 0x77, 0x3E, 0xFC, 0xE0,
                                    0x47, 0x11, 0x04, 0x01, 0x21, 0x10, 0x80, 0x1A, 0xCD, 0x95, 0x00, 0xCD, 0x96, 0x00, 0x13, 0x7B,
                                    0xFE, 0x34, 0x20, 0xF3, 0x11, 0xD8, 0x00, 0x06, 0x08, 0x1A, 0x13, 0x22, 0x23, 0x05, 0x20, 0xF9,
                                    0x3E, 0x19, 0xEA, 0x10, 0x99, 0x21, 0x2F, 0x99, 0x0E, 0x0C, 0x3D, 0x28, 0x08, 0x32, 0x0D, 0x20,
                                    0xF9, 0x2E, 0x0F, 0x18, 0xF3, 0x67, 0x3E, 0x64, 0x57, 0xE0, 0x42, 0x3E, 0x91, 0xE0, 0x40, 0x04,
                                    0x1E, 0x02, 0x0E, 0x0C, 0xF0, 0x44, 0xFE, 0x90, 0x20, 0xFA, 0x0D, 0x20, 0xF7, 0x1D, 0x20, 0xF2,
                                    0x0E, 0x13, 0x24, 0x7C, 0x1E, 0x83, 0xFE, 0x62, 0x28, 0x06, 0x1E, 0xC1, 0xFE, 0x64, 0x20, 0x06,
                                    0x7B, 0xE2, 0x0C, 0x3E, 0x87, 0xE2, 0xF0, 0x42, 0x90, 0xE0, 0x42, 0x15, 0x20, 0xD2, 0x05, 0x20,
                                    0x4F, 0x16, 0x20, 0x18, 0xCB, 0x4F, 0x06, 0x04, 0xC5, 0xCB, 0x11, 0x17, 0xC1, 0xCB, 0x11, 0x17,
                                    0x05, 0x20, 0xF5, 0x22, 0x23, 0x22, 0x23, 0xC9, 0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B,
                                    0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E,
                                    0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC,
                                    0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E, 0x3C, 0x42, 0xB9, 0xA5, 0xB9, 0xA5, 0x42, 0x3C,
                                    0x21, 0x04, 0x01, 0x11, 0xA8, 0x00, 0x1A, 0x13, 0xBE, 0x20, 0xFE, 0x23, 0x7D, 0xFE, 0x34, 0x20,
                                    0xF5, 0x06, 0x19, 0x78, 0x86, 0x23, 0x05, 0x20, 0xFB, 0x86, 0x20, 0xFE, 0x3E, 0x01, 0xE0, 0x50 };
            Cartridge = new Cartridge();
            _videoRAM = new byte[0x2000];
            _workRAM = new byte[0x2000];
            _oamRAM = new byte[160];
            _highRAM = new byte[0x100];
            IME = true;
            IsBooting = true;
        }

        #endregion

        #region Properties

        public Cartridge Cartridge { get; set; }
        public bool IsBooting { get; set; }
        public bool IsCGB
        {
            get
            {
                return Cartridge.IsCGB;
            }
        }
        public bool IME { get; set; }

        // IO Registers
        public Register P1;     // 0xFF00
        public Register SB;     // 0xFF01
        public Register SC;     // 0xFF02

        public Register DIV;    // 0xFF04
        public Register TIMA;   // 0xFF05
        public Register TMA;    // 0xFF06
        public Register TAC;    // 0xFF07

        public Register IF;     // 0xFF0F

        public Register NR30;   // 0xFF1A
        public Register NR31;   // 0xFF1B
        public Register NR32;   // 0xFF1C
        public Register NR33;   // 0xFF1D
        public Register NR34;   // 0xFF1E

        public Register LCDC;   // 0xFF40
        public Register STAT;   // 0xFF41
        public Register SCY;    // 0xFF42
        public Register SCX;    // 0xFF43
        public Register LY;     // 0xFF44
        public Register LYC;    // 0xFF45

        public Register BGP;    // 0xFF47
        public Register OBP0;   // 0xFF48
        public Register OBP1;   // 0xFF49

        public Register WY;     // 0xFF4A
        public Register WX;     // 0xFF4B

        public Register IE;     // 0xFFFF

        // Channel 3: Wave
        public bool IsChannel3Enabled
        {
            get { return Convert.ToBoolean(NR30[7]); }
            set { NR30[7] = Convert.ToInt32(value); }
        }
        public float Channel3Length
        {
            get { return (256 - NR31) * (1 / 256); } // in seconds
        }
        public int Channel3VolumeShift
        {
            get
            {
                int amount = 0;
                switch (NR32[6] << 1 + NR32[5])
                {
                    case 0:
                        amount = 4; // Mute
                        break;
                    case 1:
                        amount = 0; // 100%
                        break;
                    case 2:
                        amount = 1; // 50%
                        break;
                    case 3:
                        amount = 2; // 25%
                        break;
                }
                return amount;      // amount of right-shifts
            }
        }
        public int Channel3Frequency
        {
            get
            {
                int freq = (NR34[2] << 10) +
                            (NR34[1] << 9) +
                            (NR34[0] << 8) +
                            NR33;
                return 4194304 / (64 * (2048 - freq));  // in Hz
            }
        }


        // LCDC (0xFF40)
        public bool IsBGEnabled
        {
            get { return Convert.ToBoolean(LCDC[0]); }
            set { LCDC[0] = Convert.ToInt32(value); }
        }
        public bool IsOBJEnabled
        {
            get { return Convert.ToBoolean(LCDC[1]); }
            set { LCDC[1] = Convert.ToInt32(value); }
        }
        public int OBJSize
        {
            get { return LCDC[2]; }
            set { LCDC[2] = value; }
        }
        public int BGMap
        {
            get { return LCDC[3]; }
            set { LCDC[3] = value; }
        }
        public int BGWData
        {
            get { return LCDC[4]; }
            set { LCDC[4] = value; }
        }
        public bool IsWindowEnabled
        {
            get { return Convert.ToBoolean(LCDC[5]); }
            set { LCDC[5] = Convert.ToInt32(value); }
        }
        public int WindowMap
        {
            get { return LCDC[6]; }
            set { LCDC[6] = value; }
        }
        public bool IsLCDOn
        {
            get { return Convert.ToBoolean(LCDC[7]); }
            set { LCDC[7] = Convert.ToInt32(value); }
        }

        // STAT (0xFF41)
        public int LCDMode
        {
            get
            {
                return STAT[1] << 1 | STAT[0];
            }
            set
            {
                STAT[1] = value >> 1;
                STAT[0] = value & 1;
            }
        }
        public int LYCFlag
        {
            get { return STAT[2]; }
            set { STAT[2] = value; }
        }
        public int Mode0IE
        {
            get { return STAT[3]; }
            set { STAT[3] = value; }
        }
        public int Mode1IE
        {
            get { return STAT[4]; }
            set { STAT[4] = value; }
        }
        public int Mode2IE
        {
            get { return STAT[5]; }
            set { STAT[5] = value; }
        }
        public int LYCIE
        {
            get { return STAT[6]; }
            set { STAT[6] = value; }
        }

        #endregion

        #region Methods

        public void Reset()
        {
            //Cartridge.Reset();
            _videoRAM = new byte[0x2000];
            _workRAM = new byte[0x2000];
            _oamRAM = new byte[160];
            _highRAM = new byte[0x100];
            _divCycleCount = 0;
            _timaCycleCount = 0;
            IME = true;
            IsBooting = true;
            P1 = 0;
            SB = 0;
            SC = 0;
            DIV = 0;
            TIMA = 0;
            TMA = 0;
            TAC = 0;
            IF = 0;
            LCDC = 0;
            STAT = 0;
            SCY = 0;
            SCX = 0;
            LY = 0;
            LYC = 0;
            BGP = 0;
            OBP0 = 0;
            OBP1 = 0;
            WY = 0;
            WX = 0;
            IE = 0;
        }

        public void UpdateTimers(int cycles)
        {
            // DIV
            _divCycleCount += cycles;
            if (_divCycleCount >= 256)          // update freq = 16.384 Hz
            {
                if (DIV == byte.MaxValue)
                {
                    DIV = 0;
                }
                else
                {
                    DIV++;
                }
                _divCycleCount -= 256;
            }

            // TIMA
            if (TAC[2] == 0) { return; }        // Skip
            _timaCycleCount += cycles;
            int timaClock;
            switch (TAC[1] << 1 | TAC[0])
            {
                default:
                case 0x00:
                    timaClock = 1024; break;    // update freq = 4.096 Hz
                case 0x01:
                    timaClock = 16; break;      // update freq = 262.144 Hz
                case 0x10:
                    timaClock = 64; break;      // update freq = 65.536 Hz
                case 0x11:
                    timaClock = 256; break;     // update freq = 16.384 Hz
            }
            if (_timaCycleCount >= timaClock)
            {
                if (TIMA == byte.MaxValue)
                {
                    TIMA = TMA;
                    IF |= 0b00000100;           // Request TIMER Interrupt
                }
                else
                {
                    TIMA++;
                }
                _timaCycleCount -= timaClock;
            }
        }

        public byte Read(ushort address)
        {
            // Boot ROM
            if (address <= 0x00FF && IsBooting) return _bootROM[address];
            // Cartridge ROM
            else if (address <= 0x7FFF) return Cartridge.Read(address);
            // Video RAM
            else if (address >= 0x8000 && address <= 0x9FFF) return _videoRAM[address - 0x8000];
            // Cartridge RAM
            else if (address >= 0xA000 && address <= 0xBFFF) return Cartridge.ReadRAM((ushort)(address - 0xA000));
            // Work RAM
            else if (address >= 0xC000 && address <= 0xDFFF) return _workRAM[address - 0xC000];
            // Work RAM (Shadow)
            else if (address >= 0xE000 && address <= 0xFDFF) return _workRAM[address - 0xC000 - 0x2000];
            // OAM RAM
            else if (address >= 0xFE00 && address <= 0xFE9F) return _oamRAM[address - 0xFE00];
            // IO, High RAM
            else if (address >= 0xFF00)
            {
                if (address == 0xFF00) return P1;
                if (address == 0xFF01) return SB;
                if (address == 0xFF02) return SC;

                if (address == 0xFF04) return DIV;
                if (address == 0xFF05) return TIMA;
                if (address == 0xFF06) return TMA;
                if (address == 0xFF07) return TAC;

                if (address == 0xFF0F) return IF;

                if (address == 0xFF1A) return NR30;
                if (address == 0xFF1B) return NR31;
                if (address == 0xFF1C) return NR32;
                if (address == 0xFF1D) return NR33;
                if (address == 0xFF1E) return NR34;

                if (address == 0xFF40) return LCDC;
                if (address == 0xFF41) return STAT;
                if (address == 0xFF42) return SCY;
                if (address == 0xFF43) return SCX;
                if (address == 0xFF44) return LY;
                if (address == 0xFF45) return LYC;

                if (address == 0xFF47) return BGP;
                if (address == 0xFF48) return OBP0;
                if (address == 0xFF49) return OBP1;

                if (address == 0xFF4A) return WY;
                if (address == 0xFF4B) return WX;

                if (address == 0xFFFF) return IE;

                // else
                return _highRAM[address - 0xFF00];
            }
            else
            {
                return 0xFF;
            }
        }

        public void Write(ushort address, byte value)
        {
            // Boot ROM
            if (address <= 0x00FF && IsBooting) { }
            // Cartridge ROM
            else if (address <= 0x7FFF) Cartridge.Write(address, value);
            // Video RAM
            else if (address >= 0x8000 && address <= 0x9FFF) _videoRAM[address - 0x8000] = value;
            // Cartridge RAM
            else if (address >= 0xA000 && address <= 0xBFFF) Cartridge.WriteRAM((ushort)(address - 0xA000), value);
            // Work RAM
            else if (address >= 0xC000 && address <= 0xDFFF) _workRAM[address - 0xC000] = value;
            // Work RAM (Shadow)
            else if (address >= 0xE000 && address <= 0xFDFF) _workRAM[address - 0xC000 - 0x2000] = value;
            // OAM RAM
            else if (address >= 0xFE00 && address <= 0xFE9F) _oamRAM[address - 0xFE00] = value;
            // IO, High RAM
            else if (address >= 0xFF00)
            {
                if (address == 0xFF00)
                    P1 = value;
                if (address == 0xFF01) SB = value;
                if (address == 0xFF02) SC = value;

                if (address == 0xFF04) DIV = 0;
                if (address == 0xFF05) TAC[2] = 1 - TAC[2];
                if (address == 0xFF06) TMA = value;
                if (address == 0xFF07) TAC = value;

                if (address == 0xFF0F) IF = value;

                if (address == 0xFF1A) NR30 = value;
                if (address == 0xFF1B) NR31 = value;
                if (address == 0xFF1C) NR32 = value;
                if (address == 0xFF1D) NR33 = value;
                if (address == 0xFF1E) NR34 = value;

                if (address == 0xFF40) LCDC = value;
                if (address == 0xFF41) STAT = value;
                if (address == 0xFF42) SCY = value;
                if (address == 0xFF43) SCX = value;
                if (address == 0xFF44) LY = value;
                if (address == 0xFF45) LYC = value;
                if (address == 0xFF46) oamTransfer(value);
                if (address == 0xFF47) BGP = value;
                if (address == 0xFF48) OBP0 = value;
                if (address == 0xFF49) OBP1 = value;

                if (address == 0xFF4A) WY = value;
                if (address == 0xFF4B) WX = value;

                if (address == 0xFFFF) IE = value;

                // else
                _highRAM[address - 0xFF00] = value;
            }
            else { }
        }

        public ushort ReadWord(ushort address)
        {
            return (ushort)(Read(address) + (Read((ushort)(address + 1)) << 8));
        }

        public void WriteWord(ushort address, ushort value)
        {
            Write(address, (byte)(value & 0xFF));
            Write((ushort)(address + 1), (byte)(value >> 8));
        }

        public int GetChannel3WaveRamValue(int position)
        {
            int selectByte = position / 2;
            int shiftRight = 4;
            switch (position % 2)
            {
                case 0:
                    shiftRight = 4; // upper four bit (first)
                    break;
                case 1:
                    shiftRight = 0; // lower four bit (second)
                    break;
            }
            return Read((ushort)(0xFF30 + selectByte)) >> shiftRight & 0b1111;
        }

        private void oamTransfer(byte source)
        {
            ushort sourceAddress = (ushort)(source << 8);
            for (int i = 0; i < 160; i++)
            {
                Write((ushort)(0xFE00 + i), Read((ushort)(sourceAddress + i)));
            }
        }

        #endregion

    }
}