using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Channels;

namespace GEM.Emulation
{
    /// <summary>
    /// Representation of a physical Gameboy. Receives user input and outputs a gameboy screen (and maybe sound in future)
    /// </summary>
    internal class Gameboy
    {
        #region Fields
        private MMU _mmu;
        private CPU _cpu;
        private GPU _gpu;
        private APU _apu;
        private int _cycleCount;
        private Texture2D _nullTexture;

        // input
        public bool IsButton_A;
        public bool IsButton_B;
        public bool IsButton_Start;
        public bool IsButton_Select;
        public bool IsButton_Left;
        public bool IsButton_Right;
        public bool IsButton_Up;
        public bool IsButton_Down;
        #endregion

        #region Constructors
        public Gameboy(GraphicsDevice graphicsDevice)
        {
            _mmu = new MMU();
            _gpu = new GPU(_mmu, graphicsDevice);
            _cpu = new CPU(_mmu);
            _apu = new APU(_mmu);
            _cycleCount = 0;
            IsRunning = false;
            IsPowerOn= false;
            SwitchedOn = false;
            _nullTexture = new Texture2D(graphicsDevice, 1, 1);
            _nullTexture.SetData(new Color[] { Color.Black });
        }
        #endregion

        #region Properties
        public string CartridgeTitle
        {
            get
            {
                return _mmu.Cartridge.Title;
            }
        }
        public bool StopAfterFrame { get; set; }
        public bool StopAfterStep { get; set; }
        public bool IsRunning { get; private set; }
        public bool IsPowerOn { get; private set; }
        public bool SwitchedOn { get; private set; }
        public bool[] IsChannelOn
        {
            get
            {
                return _mmu.IsChannelOn;
            }
        }
        public bool[] MasterSwitch
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
        public bool[] IsChannelOutput
        {
            get
            {
                return _apu.IsChannelOutput;
            }
        }
        
        #endregion

        #region Methods
        public void InsertCartridge(string game)
        {
            _mmu.Cartridge.Load(game);
        }
        public void EjectCartridge(object sender, EventArgs e)
        {
            PowerOff();
            _mmu.Cartridge.Reset();
        }
        public void PowerOn()
        {
            _mmu.IsLCDOn = true;
            IsRunning = true;
        }
        public void PowerOff()
        {
            IsRunning = false;
            _mmu.IsLCDOn = false;
            _cycleCount = 0;
            _mmu.Reset();
            _cpu.Reset();
            _gpu.Reset();
            StopAfterFrame = false;
            StopAfterStep = false;
        }
        public void SaveRAM()
        {
            _mmu.Cartridge.SaveToFile();
        }
        public void Reset(object sender, EventArgs e)
        {
            PowerOff();
            PowerOn();
        }
        public void PauseToggle(object sender, EventArgs e)
        {
            IsRunning = !IsRunning;
            if (IsRunning)
            {
                _mmu.IsLCDOn = true;
            }
            StopAfterFrame = false;
            StopAfterStep = false;
        }
        public void PauseAfterFrame()
        {
            PauseToggle(this, EventArgs.Empty);
            StopAfterFrame = true;
            StopAfterStep = false;
        }
        public void PauseAfterStep()
        {
            PauseToggle(this, EventArgs.Empty);
            StopAfterFrame = false;
            StopAfterStep = true;
        }

        public void Pause()
        {
            IsRunning = false;
        }
        public void Resume()
        {
            IsRunning = true;
        }

        public void SetVolume(float volume)
        {
            _apu.MasterVolume = volume;
        }

        public Texture2D[] GetScreens(Color[] palette)
        {
            Texture2D[] screens = new Texture2D[3];

            if (!_mmu.IsLCDOn)
            {
                screens[0] = _nullTexture;
                screens[1] = _nullTexture;
                screens[2] = _nullTexture;
                return screens;
            }

            _gpu.Background.ColorGrid = renderColorData(_gpu.Background.PaletteData, palette);
            _gpu.Background.Texture.SetData(_gpu.Background.ColorGrid);
            screens[0] = _gpu.Background.Texture;

            _gpu.Window.ColorGrid = renderColorData(_gpu.Window.PaletteData, palette);
            _gpu.Window.Texture.SetData(_gpu.Window.ColorGrid);
            screens[1] = _gpu.Window.Texture;

            _gpu.Sprites.ColorGrid = renderColorData(_gpu.Sprites.PaletteData, palette);
            _gpu.Sprites.Texture.SetData(_gpu.Sprites.ColorGrid);
            screens[2] = _gpu.Sprites.Texture;


            return screens;
        }
        public Texture2D BackgroundTexture(Color[] palette)
        {
            _gpu.BackgroundMap.ColorGrid = renderColorData(_gpu.BackgroundMap.PaletteData, palette);
            _gpu.BackgroundMap.Texture.SetData(_gpu.BackgroundMap.ColorGrid);
            return _gpu.BackgroundMap.Texture;
        }
        public Texture2D WindowTexture(Color[] palette)
        {
            _gpu.WindowMap.ColorGrid = renderColorData(_gpu.WindowMap.PaletteData, palette);
            _gpu.WindowMap.Texture.SetData(_gpu.WindowMap.ColorGrid);
            return _gpu.WindowMap.Texture;
        }
        public Texture2D TilesetTexture(Color[] palette)
        {
            _gpu.TileMap.ColorGrid = renderColorData(_gpu.TileMap.PaletteData, palette);
            _gpu.TileMap.Texture.SetData(_gpu.TileMap.ColorGrid);
            return _gpu.TileMap.Texture;
        }

        // ---
        public void UpdateFrame()
        {
            if (IsRunning && Game1._Instance.IsActive)
            {
                // compute 70.224 OpCodes for 1 frame (456 T-Cycles per line @ 154 lines)
                while (_cycleCount < 70224)
                {
                    // FETCH //
                    byte opCode = _mmu.Read(_cpu.PC);

                    // DECODE //
                    //  and
                    // EXECUTE //  
                    _cpu.InstructionSet[opCode]();              // PC is pushed forward by instruction
                    if (_cpu.PC == 0x100)
                    {
                        _mmu.IsBooting = false;
                        //_cpu.A = 0x11; // unlock CGB functions
                    }

                    // UPDATE //
                    _cycleCount += _cpu.InstructionCycles;
                    _mmu.UpdateTimers(_cpu.InstructionCycles);  // Timers
                    _gpu.Update(_cpu.InstructionCycles);        // GPU
                    _apu.Update(_cpu.InstructionCycles);        // SPU

                    // SYNC //
                    if (_gpu.IsDrawTime && _cycleCount < 70224) _cycleCount = 70224 + _gpu.ModeClock;   // exits loop when screen is drawn

                    // INTERRUPTS
                    checkInterrupts();

                    IsRunning &= !StopAfterStep;
                    if (!IsRunning) break;
                }

                _cycleCount -= 70224;
                IsRunning &= !StopAfterFrame;
            }
        }
        // ---

        // Private Helper Methods
        private void checkInterrupts()
        {
            checkInputRequest();

            // On HALT-Mode: Interrupt
            if (_cpu.IsCPUHalt && (_mmu.IE & _mmu.IF) > 0)
            {
                // Exit HALT-Mode
                _cpu.IsCPUHalt = false;
                // IME not set: Continue on next opCode without serving interrrupt
                if (!_mmu.IME) _cpu.PC++;                   // TODO: Implement HALT-Bug (Next Opcode handled twice)
                                                            // IME set: Continue with standard interrupt routine below
                if (_mmu.IME) _cpu.PC++;                    // incrementing for ISR not jumping back to HALT instruction
            }

            // Standard Interrupt Routine
            if (_mmu.IME && (_mmu.IE & _mmu.IF) > 0)
            {
                checkInterrupt(0, 0x40);                    // VBlank
                checkInterrupt(1, 0x48);                    // LCD
                checkInterrupt(2, 0x50);                    // Timer
                checkInterrupt(3, 0x58);                    // Serial
                checkInterrupt(4, 0x60);                    // Input
            }
        }
        private void checkInputRequest()
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
        private void checkInterrupt(int index, ushort isrAddress)
        {
            if (_mmu.IME &&
                _mmu.IE[index] == 1 &&
                _mmu.IF[index] == 1)
            {
                _cpu.SP -= 2;                                   // save current position on stack
                _mmu.WriteWord(_cpu.SP, _cpu.PC);
                _cpu.PC = isrAddress;                           // set PC to ISR for next iteration
                _cpu.InstructionCycles = 16;
                _mmu.IME = false;                               // disable Interrupts
                _mmu.IF[index] = 0;                             // reset Flag

                // UPDATE //
                _cycleCount += _cpu.InstructionCycles;
                _mmu.UpdateTimers(_cpu.InstructionCycles);      // Timers
                _gpu.Update(_cpu.InstructionCycles);            // GPU
                _apu.Update(_cpu.InstructionCycles);            // SPU
            }
        }

        private Color[] renderColorData(int[] dataLayer, Color[] palette)
        {
            // Returns Color Layer from (Palette-)Data Layer
            int num = dataLayer.GetLength(0);
            Color[] colorData = new Color[num];
            for (int i = 0; i < num; i++)
            {
                if (dataLayer[i] >= 0)
                {
                    colorData[i] = palette[dataLayer[i]];
                }
                else
                {
                    colorData[i] = Color.Transparent;
                }
            }
            return colorData;
        }
        #endregion

    }
}
