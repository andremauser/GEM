using GEM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GEM
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
        //private SPU _spu;
        private int _cycleCount;
        private Texture2D _nullTexture;

        #endregion

        #region Constructors

        public Gameboy(GraphicsDevice graphicsDevice)
        {
            _mmu = new MMU();
            _gpu = new GPU(_mmu, graphicsDevice);
            _cpu = new CPU(_mmu);
            //_spu = new SPU(_mmu);
            _cycleCount = 0;
            Running = false;
            SwitchedOn = false;
            _nullTexture = new Texture2D(graphicsDevice,1,1);
            _nullTexture.SetData<Color>(new Color[] { Color.Black });
        }

        #endregion

        #region Properties
        public string DebugInfo
        {
            get
            {
                return string.Format("PC  =  {0:X4} 0x{1:X2} \n\n" +
                                     "AF  =  {3:X4} {4}{5}{6}{7}\n" +
                                     "BC  =  {8:X4}\n" +
                                     "DE  =  {9:X4}\n" +
                                     "HL  =  {10:X4}\n" +
                                     "SP  =  {11:X4}\n\n" +
                                     "IME =  {12}\n" +
                                     "IE  =  {13}\n" +
                                     "IF  =  {14}\n" +
                                     "P1  =  {15}\n\n" +
                                     "   {16}           {20}\n" +
                                     " {19}   {17}       {21}\n" +
                                     "   {18}   {22} {23}\n",
                                     _cpu.PC, _mmu.Read(_cpu.PC), null,
                                     _cpu.AF, _cpu.FlagZ == 1 ? "Z" : " ", _cpu.FlagN == 1 ? "N" : " ", _cpu.FlagH == 1 ? "H" : " ", _cpu.FlagC == 1 ? "C" : " ",
                                     _cpu.BC,
                                     _cpu.DE,
                                     _cpu.HL,
                                     _cpu.SP,
                                     _mmu.IME,
                                     Convert.ToString(_mmu.IE, 2).PadLeft(8, '0'),
                                     Convert.ToString(_mmu.IF, 2).PadLeft(8, '0'),
                                     Convert.ToString(_mmu.P1, 2).PadLeft(8, '0'),
                                     Input.IsButton_Up ? "U" : "-",
                                     Input.IsButton_Right ? "R" : "-",
                                     Input.IsButton_Down ? "D" : "-",
                                     Input.IsButton_Left ? "L" : "-",
                                     Input.IsButton_A ? "A" : "-",
                                     Input.IsButton_B ? "B" : "-",
                                     Input.IsButton_Select ? "SE" : "--",
                                     Input.IsButton_Start ? "ST" : "--"
                                     );
            }
        }
        public string CartridgeTitle
        {
            get
            {
                return _mmu.Cartridge.Title;
            }
        }
        public bool StopAfterFrame { get; set; }
        public bool StopAfterStep { get; set; }
        public bool Running { get; private set; }
        public bool SwitchedOn { get; private set; }

        #endregion

        #region Methods

        public void InsertCartridge(string game)
        {
            _mmu.Cartridge.Load(game);
        }
        public void PowerOn()
        {
            _mmu.IsLCDOn = true;
            Running = true;
        }
        public void PowerOff()
        {
            _mmu.Cartridge.SaveToFile();
            Running = false;
            _mmu.IsLCDOn = false;
            _cycleCount = 0;
            _mmu.Reset();
            _cpu.Reset();
            _gpu.Reset();
            StopAfterFrame = false;
            StopAfterStep = false;
        }
        public void PauseSwitch()
        {
            Running = !Running;
            StopAfterFrame = false;
            StopAfterStep = false;
        }
        public void PauseAfterFrame()
        {
            PauseSwitch();
            StopAfterFrame = true;
            StopAfterStep = false;
        }
        public void PauseAfterStep()
        {
            PauseSwitch();
            StopAfterFrame = false;
            StopAfterStep = true;
        }

        public Texture2D GetScreen(Color[] palette)
        {
            if (!_mmu.IsLCDOn)
            {
                return _nullTexture;
            }
            _gpu.Screen.ColorGrid = renderColorData(_gpu.Screen.PaletteData, palette);
            _gpu.Screen.Texture.SetData(_gpu.Screen.ColorGrid);
            return _gpu.Screen.Texture;
        }
        public Texture2D BackgroundTexture(Color[] palette)
        {
            _gpu.Background.ColorGrid = renderColorData(_gpu.Background.PaletteData, palette);
            _gpu.Background.Texture.SetData(_gpu.Background.ColorGrid);
            return _gpu.Background.Texture;
        }
        public Texture2D WindowTexture(Color[] palette)
        {
            _gpu.Window.ColorGrid = renderColorData(_gpu.Window.PaletteData, palette);
            _gpu.Window.Texture.SetData(_gpu.Window.ColorGrid);
            return _gpu.Window.Texture;
        }
        public Texture2D TilesetTexture(Color[] palette)
        {
            _gpu.Tileset.ColorGrid = renderColorData(_gpu.Tileset.PaletteData, palette);
            _gpu.Tileset.Texture.SetData(_gpu.Tileset.ColorGrid);
            return _gpu.Tileset.Texture;
        }

        public void UpdateFrame()
        {
            if (Running)
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
                    if (_cpu.PC == 0x100) _mmu.IsBooting = false;

                    // UPDATE //
                    _cycleCount += _cpu.InstructionCycles;
                    _mmu.UpdateTimers(_cpu.InstructionCycles);  // Timers
                    _gpu.Update(_cpu.InstructionCycles);        // GPU
                    //_spu.Update(_cpu.InstructionCycles);        // SPU

                    // SYNC //
                    if (_gpu.IsDrawTime && _cycleCount < 70224) _cycleCount = 70224 + _gpu.ModeClock;   // exits loop when screen is drawn

                    // INTERRUPTS
                    checkInterrupts();


                    Running &= !StopAfterStep;
                    if (!Running) break;
                }

                _cycleCount -= 70224;
                Running &= !StopAfterFrame;
            }
        }


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
                if (Input.IsButton_Right) { _mmu.IF[4] = 1; _mmu.P1[0] = 0; }
                if (Input.IsButton_Left) { _mmu.IF[4] = 1; _mmu.P1[1] = 0; }
                if (Input.IsButton_Up) { _mmu.IF[4] = 1; _mmu.P1[2] = 0; }
                if (Input.IsButton_Down) { _mmu.IF[4] = 1; _mmu.P1[3] = 0; }
            }
            if (_mmu.P1[4] == 1 && _mmu.P1[5] == 0)
            {
                if (Input.IsButton_A) { _mmu.IF[4] = 1; _mmu.P1[0] = 0; }
                if (Input.IsButton_B) { _mmu.IF[4] = 1; _mmu.P1[1] = 0; }
                if (Input.IsButton_Select) { _mmu.IF[4] = 1; _mmu.P1[2] = 0; }
                if (Input.IsButton_Start) { _mmu.IF[4] = 1; _mmu.P1[3] = 0; }
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
                //_spu.Update(_cpu.InstructionCycles);        // SPU
            }
        }

        private Color[] renderColorData(int[] dataLayer, Color[] palette)
        {
            // Returns Color Layer from (Palette-)Data Layer
            int num = dataLayer.GetLength(0);
            Color[] colorData = new Color[num];
            for (int i = 0; i < num; i++)
            {
                colorData[i] = palette[dataLayer[i]];
            }
            return colorData;
        }

        #endregion

    }
}
