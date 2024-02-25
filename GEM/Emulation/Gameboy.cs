using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GEM.Emulation
{
    /// <summary>
    /// Representation of a physical Gameboy. Receives user input and outputs a gameboy screen
    /// </summary>
    public class Gameboy
    {
        public enum Mode
        {
            GB, GBC
        }

        #region Fields
        int _frameCycles;
        Texture2D _nullTexture;
        Mode _gbMode;

        MMU _mmu;
        CPU _cpu;
        PPU _ppu;
        APU _apu;

        // input
        bool _isButton_Left;
        bool _isButton_Right;
        bool _isButton_Up;
        bool _isButton_Down;

        // events
        public event EventHandler OnPowerOn;
        #endregion

        #region Constructors
        public Gameboy(GraphicsDevice graphicsDevice)
        {
            Cartridge = new Cartridge();
            _mmu = new MMU(Cartridge);
            _ppu = new PPU(_mmu, graphicsDevice);
            _cpu = new CPU(_mmu);
            _apu = new APU(_mmu);
            _frameCycles = 0;
            IsRunning = false;
            IsPowerOn= false;
            _nullTexture = new Texture2D(graphicsDevice, 1, 1);
            _nullTexture.SetData(new Color[] { Color.Black });
        }
        #endregion

        #region Properties

        public Mode GbMode
        {
            get
            {
                return _gbMode;
            }
            set
            {
                _gbMode = value;
                _mmu.SetBootROM(_gbMode);
            }
        }

        public bool[] AudioMasterSwitch
        {
            get
            {
                return _apu.MasterSwitch;
            }
            set
            {
                _apu.MasterSwitch = value;
            }
        }
        public bool[] IsAudioChannelOn { get { return _apu.IsChannelOn; } }
        public bool[] IsAudioChannelOutput { get { return _apu.IsChannelOutput; } }

        public bool IsRunning { get; private set; }
        public bool IsPowerOn { get; private set; }
        public Cartridge Cartridge { get; private set; }

        // input
        public bool IsButton_A { get; set; }
        public bool IsButton_B { get; set; }
        public bool IsButton_Start { get; set; }
        public bool IsButton_Select { get; set; }
        public bool IsButton_Left
        {
            get { return _isButton_Left && !_isButton_Right; }
            set { _isButton_Left = value; }
        }
        public bool IsButton_Right
        {
            get { return _isButton_Right && !_isButton_Left; }
            set { _isButton_Right = value; }
        }
        public bool IsButton_Up
        {
            get { return _isButton_Up && !_isButton_Down; }
            set { _isButton_Up = value;}
        }
        public bool IsButton_Down
        {
            get { return _isButton_Down && !_isButton_Up; }
            set { _isButton_Down = value; }
        }

        // output
        public Texture2D BGTexture
        {
            get
            {
                return _ppu.BGTexture;
            }
        }
        public Texture2D WDTexture
        {
            get
            {
                return _ppu.WDTexture;
            }
        }
        public Texture2D OBTexture
        {
            get
            {
                return _ppu.OBTexture;
            }
        }
        #endregion

        #region Methods
        // Gameboy functionality
        public void InsertCartridge(string game)
        {
            Cartridge.LoadFromFile(game);
            GbMode = Cartridge.FileType == "gbc" ? Mode.GBC : Mode.GB;
        }
        public void EjectCartridge(object sender, EventArgs e)
        {
            PowerOff();
            Cartridge.Reset();
        }
        public void PowerOn()
        {
            _mmu.IsLCDOn = true;
            IsRunning = true;
            OnPowerOn?.Invoke(this, EventArgs.Empty);
        }
        public void PowerOff()
        {
            IsRunning = false;
            _mmu.IsLCDOn = false;
            _frameCycles = 0;
            _mmu.Reset();
            _cpu.Reset();
            _ppu.Reset();
        }
        public void SetVolume(float volume)
        {
            _apu.MasterVolume = volume;
        }

        // ---
        public void Update()
        {
            // update GB frame
            if (IsRunning && Game1._Instance.IsActive)
            {
                // compute opCodes for 1 frame (456 T-Cycles per line @ 154 lines = 70.224)
                while (_frameCycles < 70224)
                {
                    _cpu.Run();
                    updateComponents();
                    updateInterrupts();
                    if (!IsRunning) break;
                }
                _frameCycles -= 70224;
            }
        }
        // ---

        // Private Helper Methods
        private void updateInterrupts()
        {
            updateInputFlags();

            // On HALT-Mode: Interrupt
            if (_cpu.IsCPUHalt && (_mmu.IE & _mmu.IF) > 0)
            {
                _cpu.Exit_HALT();
            }

            // Standard Interrupt Routine
            if (_mmu.IME && (_mmu.IE & _mmu.IF) > 0)
            {
                handleInterrupt(0, 0x40);                    // VBlank
                handleInterrupt(1, 0x48);                    // LCD
                handleInterrupt(2, 0x50);                    // Timer
                handleInterrupt(3, 0x58);                    // Serial
                handleInterrupt(4, 0x60);                    // Input
            }
        }
        private void updateInputFlags()
        {
            // Handle Input (0 = pressed)

            // else case:
            _mmu.P1 |= 0b11001111;
            // INPUT interrupt
            if (_mmu.P1[4] == 0 && _mmu.P1[5] == 1)
            {
                if (IsButton_Right) { _mmu.IF[4] = 1; _mmu.P1[0] = 0; }
                if (IsButton_Left) { _mmu.IF[4] = 1; _mmu.P1[1] = 0; }
                if (IsButton_Up) { _mmu.IF[4] = 1; _mmu.P1[2] = 0; }
                if (IsButton_Down) { _mmu.IF[4] = 1; _mmu.P1[3] = 0; }
            }
            if (_mmu.P1[4] == 1 && _mmu.P1[5] == 0)
            {
                if (IsButton_A) { _mmu.IF[4] = 1; _mmu.P1[0] = 0; }
                if (IsButton_B) { _mmu.IF[4] = 1; _mmu.P1[1] = 0; }
                if (IsButton_Select) { _mmu.IF[4] = 1; _mmu.P1[2] = 0; }
                if (IsButton_Start) { _mmu.IF[4] = 1; _mmu.P1[3] = 0; }
            }
        }
        private void handleInterrupt(int index, ushort isrAddress)
        {
            if (_mmu.IME &&
                _mmu.IE[index] == 1 &&
                _mmu.IF[index] == 1)
            {
                _cpu.Jump_ISR(isrAddress);                       // jump to ISR
                _mmu.IME = false;                              // disable Interrupts
                _mmu.IF[index] = 0;                            // reset Flag
                updateComponents();
            }
        }
        private void updateComponents()
        {
            _frameCycles += _cpu.InstructionCycles;
            _mmu.Update(_cpu.InstructionCycles);            // Timers
            _ppu.Update(_cpu.InstructionCycles);            // GPU
            _apu.Update(_cpu.InstructionCycles);            // SPU
        }

        // public Helper Methods
        public void SaveRAM()
        {
            Cartridge.SaveToFile();
        }
        #endregion

    }
}
