﻿using System;
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
        // property fields
        Register _nr12;
        Register _nr14;
        Register _nr22;
        Register _nr24;
        Register _nr30;
        Register _nr34;
        Register _nr42;
        Register _nr44;
        public EventHandler CH1TriggerEvent;
        public EventHandler CH2TriggerEvent;
        public EventHandler CH3TriggerEvent;
        public EventHandler CH4TriggerEvent;
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

        public bool[] IsChannelOn
        {
            get
            {
                return new bool[] { IsCH1On,  IsCH2On, IsCH3On, IsCH4On };
            }
        }

        #region IO Registers
        public Register P1;     // 0xFF00
        public Register SB;     // 0xFF01
        public Register SC;     // 0xFF02

        public Register DIV;    // 0xFF04
        public Register TIMA;   // 0xFF05
        public Register TMA;    // 0xFF06
        public Register TAC;    // 0xFF07

        public Register IF;     // 0xFF0F
        public Register NR10;   // 0xFF10   Sound CH1 sweep
        public Register NR11;   // 0xFF11   Sound CH1 length + duty cycle
        public Register NR12    // 0xFF12   Sound CH1 volume + envelope
        {
            get
            {
                return _nr12;
            }
            set
            {
                _nr12 = value;
                if ((_nr12 & 0b11111000) == 0) IsCH1On = false; // turn DAC off
            }
        }
        public Register NR13;   // 0xFF13   Sound CH1 frequency low
        public Register NR14    // 0xFF14   Sound CH1 frequency high + control
        {
            get
            {
                return _nr14;
            }
            set
            {
                _nr14 = value;
                if (CH1Trigger == 1)
                { 
                    // trigger channel
                    IsCH1On = true;
                    CH1TriggerEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Register NR21;   // 0xFF16   Sound CH2 length + duty cycle
        public Register NR22    // 0xFF17   Sound CH2 volume + envelope
        {
            get
            {
                return _nr22;
            }
            set
            {
                _nr22 = value;
                if ((_nr22 & 0b11111000) == 0) IsCH2On = false; // turn DAC off
            }
        }
        public Register NR23;   // 0xFF18   Sound CH2 frequency low
        public Register NR24    // 0xFF19   Sound CH2 frequency high + control
        {
            get
            {
                return _nr24;
            }
            set
            {
                _nr24 = value; 
                if (CH2Trigger == 1)
                {
                    // trigger channel
                    IsCH2On = true;
                    CH2TriggerEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Register NR30    // 0xFF1A   Sound CH3 DAC
        {
            get
            {
                return _nr30;
            }
            set
            {
                _nr30 = value;
                if (_nr30[7] == 0) IsCH3On = false; // turn DAC off
            }
        }
        public Register NR31;   // 0xFF1B   Sound CH3 length
        public Register NR32;   // 0xFF1C   Sound CH3 volume
        public Register NR33;   // 0xFF1D   Sound CH3 frequency low
        public Register NR34    // 0xFF1E   Sound CH3 frequency high + control
        {
            get
            {
                return _nr34;
            }
            set
            {
                _nr34 = value;
                if (CH3Trigger == 1)
                {
                    IsCH3On = true;
                    CH3TriggerEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Register NR41;   // 0xFF20   Sound CH4 length
        public Register NR42    // 0xFF21   Sound CH4 volume + envelope
        {
            get
            {
                return _nr42;
            }
            set
            {
                _nr42 = value;
                if ((_nr42 & 0b11111000) == 0) IsCH4On = false; // turn DAC off
            }
        }
        public Register NR43;   // 0xFF22   Sound CH4 frequency + randomness
        public Register NR44    // 0xFF23   Sound CH4 control
        {
            get
            {
                return _nr44;
            }
            set
            {
                _nr44 = value;
                if (CH4Trigger == 1)
                {
                    IsCH4On = true;
                    CH4TriggerEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Register NR50;   // 0xFF24   Sound volume left/right
        public Register NR51;   // 0xFF25   Sound mix to left/right
        public Register NR52;   // 0xFF26   Sound on/off and status

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
        #endregion

        #region Bit Properties
        // attention: Don't put logic in here! Bit properties only bypass to IO registers above

        // NR10 (0xFF10)
        public int CH1SweepShifts
        {
            // sweep slope / change rate (0-7)
            get
            {
                return NR10[2] << 2 | NR10[1] << 1 | NR10[0];
            }
            set
            {
                NR10[2] = (value >> 2) & 1;
                NR10[1] = (value >> 1) & 1;
                NR10[0] = (value >> 0) & 1;
            }
        }
        public int CH1SweepDirection
        {
            // 0: Addition      (wavelength increase)
            // 1: Subtraction   (wavelength decrease)
            get { return NR10[3]; }
            set { NR10[3] = value; }
        }
        public int CH1SweepTime
        {
            // sweep pace / step time (0-7)*(128Hz-tick)
            get
            {
                return NR10[6] << 2 | NR10[5] << 1 | NR10[4];
            }
            set
            {
                NR10[6] = (value >> 2) & 1;
                NR10[5] = (value >> 1) & 1;
                NR10[4] = (value >> 0) & 1;
            }
        }

        // NR11 (0xFF11)
        public int CH1LengthData
        {
            // initial value for length timer (0-63)
            get
            {
                return  NR11[5] << 5 | 
                        NR11[4] << 4 | 
                        NR11[3] << 3 | 
                        NR11[2] << 2 | 
                        NR11[1] << 1 | 
                        NR11[0];
            }
            set
            {
                NR11[5] = (value >> 5) & 1;
                NR11[4] = (value >> 4) & 1;
                NR11[3] = (value >> 3) & 1;
                NR11[2] = (value >> 2) & 1;
                NR11[1] = (value >> 1) & 1;
                NR11[0] = (value >> 0) & 1;
            }
        }
        public int CH1WaveDuty
        {
            // wave duty (0-3)
            get
            {
                return NR11[7] << 1 | NR11[6];
            }
            set
            {
                NR11[7] = (value >> 1) & 1;
                NR11[6] = (value >> 0) & 1;
            }
        }

        // NR12 (0xFF12)
        public int CH1EnvelopeTime
        {
            // envelope pace / step time (0-7)*(64Hz-tick)
            get
            {
                return NR12[2] << 2 | NR12[1] << 1 | NR12[0];
            }
            set
            {
                _nr12[2] = (value >> 2) & 1;
                _nr12[1] = (value >> 1) & 1;
                _nr12[0] = (value >> 0) & 1;
            }
        }
        public int CH1EnvelopeDirection
        {
            // 0: volume decrease
            // 1: volume increase
            get { return NR12[3]; }
            set { _nr12[3] = value; }
        }
        public int CH1VolumeStart
        {
            // envelope initial volume
            get
            {
                return NR12[7] << 3 |
                       NR12[6] << 2 |
                       NR12[5] << 1 |
                       NR12[4];
            }
            set
            {
                _nr12[7] = (value >> 3) & 1;
                _nr12[6] = (value >> 2) & 1;
                _nr12[5] = (value >> 1) & 1;
                _nr12[4] = (value >> 0) & 1;
            }
        }

        // NR13 (0xFF13)
        // NR14 (0xFF14)
        public int CH1Frequency
        {
            get
            {
                return NR14[2] << 10 |
                       NR14[1] << 9 |
                       NR14[0] << 8 |
                       NR13;
            }
            set
            {
                _nr14[2] = (value >> 10) & 1;
                _nr14[1] = (value >> 9) & 1;
                _nr14[0] = (value >> 8) & 1;
                NR13 = (byte)(value & 0xFF);
            }
        }
        public bool IsCH1LengthEnabled
        {
            get { return Convert.ToBoolean(NR14[6]); }
            set { _nr14[6] = Convert.ToInt32(value); }
        }
        public int CH1Trigger
        {
            // 1: Restart channel
            get { return NR14[7]; }
            set { _nr14[7] = value; }
        }

        // NR21 (0xFF16)
        public int CH2LengthData
        {
            // initial value for length timer (0-63)
            get
            {
                return  NR21[5] << 5 |
                        NR21[4] << 4 |
                        NR21[3] << 3 |
                        NR21[2] << 2 |
                        NR21[1] << 1 |
                        NR21[0];
            }
            set
            {
                NR21[5] = (value >> 5) & 1;
                NR21[4] = (value >> 4) & 1;
                NR21[3] = (value >> 3) & 1;
                NR21[2] = (value >> 2) & 1;
                NR21[1] = (value >> 1) & 1;
                NR21[0] = (value >> 0) & 1;
            }
        }
        public int CH2WaveDuty
        {
            // wave duty (0-3)
            get
            {
                return NR21[7] << 1 | NR21[6];
            }
            set
            {
                NR21[7] = (value >> 1) & 1;
                NR21[6] = (value >> 0) & 1;
            }
        }

        // NR22 (0xFF17)
        public int CH2EnvelopeTime
        {
            // envelope pace / step time (0-7)*(64Hz-tick)
            get
            {
                return NR22[2] << 2 | NR22[1] << 1 | NR22[0];
            }
            set
            {
                _nr22[2] = (value >> 2) & 1;
                _nr22[1] = (value >> 1) & 1;
                _nr22[0] = (value >> 0) & 1;
            }
        }
        public int CH2EnvelopeDirection
        {
            // 0: volume decrease
            // 1: volume increase
            get { return NR22[3]; }
            set { _nr22[3] = value; }
        }
        public int CH2VolumeStart
        {
            // envelope initial volume
            get
            {
                return NR22[7] << 3 |
                       NR22[6] << 2 |
                       NR22[5] << 1 |
                       NR22[4];
            }
            set
            {
                _nr22[7] = (value >> 3) & 1;
                _nr22[6] = (value >> 2) & 1;
                _nr22[5] = (value >> 1) & 1;
                _nr22[4] = (value >> 0) & 1;
            }
        }

        // NR23 (0xFF18)
        // NR24 (0xFF19)
        public int CH2Frequency
        {
            get
            {
                return NR24[2] << 10 |
                       NR24[1] << 9 |
                       NR24[0] << 8 |
                       NR23;
            }
            set
            {
                _nr24[2] = (value >> 10) & 1;
                _nr24[1] = (value >> 9) & 1;
                _nr24[0] = (value >> 8) & 1;
                 NR23 = (byte)(value & 0xFF);
            }
        }
        public bool IsCH2LengthEnabled
        {
            get { return Convert.ToBoolean(NR24[6]); }
            set { _nr24[6] = Convert.ToInt32(value); }
        }
        public int CH2Trigger
        {
            // 1: Restart channel
            get { return NR24[7]; }
            set { _nr24[7] = value; }
        }

        // NR31 (0xFF1B)
        public int CH3LengthData
        {
            // initial value for length timer (0-255)
            get
            {
                return NR31;
            }
            set
            {
                NR31 = (byte)value;
            }
        }

        // NR32 (0xFF1C)
        public int CH3VolumeSelect
        {
            /* 
             * output level (0-3)
             * 0: mute
             * 1: 100%
             * 2: 50% (>>1)
             * 3: 25% (>>2)
            */
            get
            {
                return NR32[6] << 1 | NR32[5];
            }
            set
            {
                NR32[6] = (value >> 1) & 1;
                NR32[5] = (value >> 0) & 1;
            }
        }

        // NR34 (0xFF1D)
        // NR33 (0xFF1E)
        public int CH3Frequency
        {
            get
            {
                return NR34[2] << 10 |
                       NR34[1] << 9 |
                       NR34[0] << 8 |
                       NR33;
            }
            set
            {
                _nr34[2] = (value >> 10) & 1;
                _nr34[1] = (value >> 9) & 1;
                _nr34[0] = (value >> 8) & 1;
                NR33 = (byte)(value & 0xFF);
            }
        }
        public bool IsCH3LengthEnabled
        {
            get { return Convert.ToBoolean(NR34[6]); }
            set { _nr34[6] = Convert.ToInt32(value); }
        }
        public int CH3Trigger
        {
            // 1: Restart channel
            get { return NR34[7]; }
            set { _nr34[7] = value; }
        }

        // NR41 (0xFF20)
        public int CH4LengthData
        {
            // initial value for length timer (0-63)
            get
            {
                return  NR41[5] << 5 |
                        NR41[4] << 4 |
                        NR41[3] << 3 |
                        NR41[2] << 2 |
                        NR41[1] << 1 |
                        NR41[0];
            }
            set
            {
                NR41[5] = (value >> 5) & 1;
                NR41[4] = (value >> 4) & 1;
                NR41[3] = (value >> 3) & 1;
                NR41[2] = (value >> 2) & 1;
                NR41[1] = (value >> 1) & 1;
                NR41[0] = (value >> 0) & 1;
            }
        }

        // NR42 (0xFF21)
        public int CH4EnvelopeTime
        {
            // envelope pace / step time (0-7)*(64Hz-tick)
            get
            {
                return NR42[2] << 2 | NR42[1] << 1 | NR42[0];
            }
            set
            {
                _nr42[2] = (value >> 2) & 1;
                _nr42[1] = (value >> 1) & 1;
                _nr42[0] = (value >> 0) & 1;
            }
        }
        public int CH4EnvelopeDirection
        {
            // 0: volume decrease
            // 1: volume increase
            get { return NR42[3]; }
            set { _nr42[3] = value; }
        }
        public int CH4VolumeStart
        {
            // envelope initial volume
            get
            {
                return NR42[7] << 3 |
                       NR42[6] << 2 |
                       NR42[5] << 1 |
                       NR42[4];
            }
            set
            {
                _nr42[7] = (value >> 3) & 1;
                _nr42[6] = (value >> 2) & 1;
                _nr42[5] = (value >> 1) & 1;
                _nr42[4] = (value >> 0) & 1;
            }
        }

        // NR43 (0xFF22)
        public int CH4ClockDivider
        {
            get
            {
                return NR43[2] << 2 | NR43[1] << 1 | NR43[0];
            }
            set
            {
                NR43[2] = (value >> 2) & 1;
                NR43[1] = (value >> 1) & 1;
                NR43[0] = (value >> 0) & 1;
            }
        }
        public int CH4WidthMode
        {
            // 0: 15 bits
            // 1:  7 bits
            get { return NR43[3]; }
            set { NR43[3] = value; }
        }
        public int CH4ClockShift
        {
            get
            {
                return NR43[7] << 3 |
                       NR43[6] << 2 |
                       NR43[5] << 1 |
                       NR43[4];
            }
            set
            {
                NR43[7] = (value >> 3) & 1;
                NR43[6] = (value >> 2) & 1;
                NR43[5] = (value >> 1) & 1;
                NR43[4] = (value >> 0) & 1;
            }
        }

        // NR44 (0xFF23)
        public bool IsCH4LengthEnabled
        {
            get { return Convert.ToBoolean(NR44[6]); }
            set { _nr44[6] = Convert.ToInt32(value); }
        }
        public int CH4Trigger
        {
            // 1: Restart channel
            get { return NR44[7]; }
            set { _nr44[7] = value; }
        }

        // NR50 (0xFF24)
        public int VolumeRight
        {
            get
            {
                return NR50[2] << 2 | NR50[1] << 1 | NR50[0];
            }
            set
            {
                NR50[2] = (value >> 2) & 1;
                NR50[1] = (value >> 1) & 1;
                NR50[0] = (value >> 0) & 1;
            }
        }
        public bool IsVinRight
        {
            get { return Convert.ToBoolean(NR50[3]); }
            set { NR50[3] = Convert.ToInt32(value); }
        }
        public int VolumeLeft
        {
            get
            {
                return NR50[6] << 2 | NR50[5] << 1 | NR50[4];
            }
            set
            {
                NR50[6] = (value >> 2) & 1;
                NR50[5] = (value >> 1) & 1;
                NR50[4] = (value >> 0) & 1;
            }
        }
        public bool IsVinLeft
        {
            get { return Convert.ToBoolean(NR50[7]); }
            set { NR50[7] = Convert.ToInt32(value); }
        }

        // NR51 (0xFF25)
        public bool IsCH1Right
        {
            get { return Convert.ToBoolean(NR51[0]); }
            set { NR51[0] = Convert.ToInt32(value); }
        }
        public bool IsCH2Right
        {
            get { return Convert.ToBoolean(NR51[1]); }
            set { NR51[1] = Convert.ToInt32(value); }
        }
        public bool IsCH3Right
        {
            get { return Convert.ToBoolean(NR51[2]); }
            set { NR51[2] = Convert.ToInt32(value); }
        }
        public bool IsCH4Right
        {
            get { return Convert.ToBoolean(NR51[3]); }
            set { NR51[3] = Convert.ToInt32(value); }
        }
        public bool IsCH1Left
        {
            get { return Convert.ToBoolean(NR51[4]); }
            set { NR51[4] = Convert.ToInt32(value); }
        }
        public bool IsCH2Left
        {
            get { return Convert.ToBoolean(NR51[5]); }
            set { NR51[5] = Convert.ToInt32(value); }
        }
        public bool IsCH3Left
        {
            get { return Convert.ToBoolean(NR51[6]); }
            set { NR51[6] = Convert.ToInt32(value); }
        }
        public bool IsCH4Left
        {
            get { return Convert.ToBoolean(NR51[7]); }
            set { NR51[7] = Convert.ToInt32(value); }
        }

        // NR52 (0xFF26)
        public bool IsCH1On
        {
            get { return Convert.ToBoolean(NR52[0]); }
            /* 
             * read-only
             * 
             * set ON when:
             * - triggered
             * 
             * set OFF when:
             * - length timer expired
             * - sweep overflow (CH1)
             * - DAC turned off
            */
            set { NR52[0] = Convert.ToInt32(value); }
        }
        public bool IsCH2On
        {
            get { return Convert.ToBoolean(NR52[1]); }
            set { NR52[1] = Convert.ToInt32(value); }
        }
        public bool IsCH3On
        {
            /*
             * set ON when:
             * - triggered
             * 
             * set OFF when:
             * - DAC turned off
             * - length timer
            */
            get { return Convert.ToBoolean(NR52[2]); }
            set { NR52[2] = Convert.ToInt32(value); }
        }
        public bool IsCH4On
        {
            get { return Convert.ToBoolean(NR52[3]); }
            set { NR52[3] = Convert.ToInt32(value); }
        }
        public bool IsSoundOn
        {
            get { return Convert.ToBoolean(NR52[7]); }
            set { NR52[7] = Convert.ToInt32(value); }
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
                STAT[1] = (value >> 1) & 1;
                STAT[0] = (value >> 0) & 1;
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
            IsCH1On = false;
            IsCH2On = false;
            IsCH3On = false;
            IsCH4On = false;
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

                if (address == 0xFF10) return NR10;
                if (address == 0xFF11) return NR11;
                if (address == 0xFF12) return NR12;
                if (address == 0xFF13) return NR13;
                if (address == 0xFF14) return NR14;

                if (address == 0xFF16) return NR21;
                if (address == 0xFF17) return NR22;
                if (address == 0xFF18) return NR23;
                if (address == 0xFF19) return NR24;

                if (address == 0xFF1A) return NR30;
                if (address == 0xFF1B) return NR31;
                if (address == 0xFF1C) return NR32;
                if (address == 0xFF1D) return NR33;
                if (address == 0xFF1E) return NR34;

                if (address == 0xFF20) return NR41;
                if (address == 0xFF21) return NR42;
                if (address == 0xFF22) return NR43;
                if (address == 0xFF23) return NR44;

                if (address == 0xFF24) return NR50;
                if (address == 0xFF25) return NR51;
                if (address == 0xFF26) return NR52;

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

                if (address == 0xFF10) NR10 = value;
                if (address == 0xFF11) NR11 = value;
                if (address == 0xFF12) NR12 = value;
                if (address == 0xFF13) NR13 = value;
                if (address == 0xFF14) NR14 = value;

                if (address == 0xFF16) NR21 = value;
                if (address == 0xFF17) NR22 = value;
                if (address == 0xFF18) NR23 = value;
                if (address == 0xFF19) NR24 = value;

                if (address == 0xFF1A) NR30 = value;
                if (address == 0xFF1B) NR31 = value;
                if (address == 0xFF1C) NR32 = value;
                if (address == 0xFF1D) NR33 = value;
                if (address == 0xFF1E) NR34 = value;

                if (address == 0xFF20) NR41 = value;
                if (address == 0xFF21) NR42 = value;
                if (address == 0xFF22) NR43 = value;
                if (address == 0xFF23) NR44 = value;

                if (address == 0xFF24) NR50 = value;
                if (address == 0xFF25) NR51 = value;
                if (address == 0xFF26) NR52 = (byte)(value & 0b11110000); // bits 0-3 read-only

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