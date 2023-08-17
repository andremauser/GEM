using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GEM.Emulation
{
    /// <summary>
    /// Representation of a physical Gameboy. Receives user input and outputs a gameboy screen (and maybe sound in future)
    /// </summary>
    internal class Gameboy
    {
        #region Fields
        int _cycleCount;
        Texture2D _nullTexture;

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
            MMU = new MMU();
            GPU = new GPU(MMU, graphicsDevice);
            CPU = new CPU(MMU);
            APU = new APU(MMU);
            _cycleCount = 0;
            IsRunning = false;
            IsPowerOn= false;
            _nullTexture = new Texture2D(graphicsDevice, 1, 1);
            _nullTexture.SetData(new Color[] { Color.Black });
        }
        #endregion

        #region Properties
        public MMU MMU { get; private set; }
        public CPU CPU { get; private set; }
        public GPU GPU { get; private set; }
        public APU APU { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsPowerOn { get; private set; }

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
        #endregion

        #region Methods
        public void InsertCartridge(string game)
        {
            MMU.Cartridge.Load(game);
        }
        public void EjectCartridge(object sender, EventArgs e)
        {
            PowerOff();
            MMU.Cartridge.Reset();
        }
        public void PowerOn()
        {
            MMU.IsLCDOn = true;
            IsRunning = true;
            OnPowerOn?.Invoke(this, EventArgs.Empty);
        }
        public void PowerOff()
        {
            IsRunning = false;
            MMU.IsLCDOn = false;
            _cycleCount = 0;
            MMU.Reset();
            CPU.Reset();
            GPU.Reset();
        }
        public void SaveRAM()
        {
            MMU.Cartridge.SaveToFile();
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
                MMU.IsLCDOn = true;
            }
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
            APU.MasterVolume = volume;
        }

        // ---
        public void UpdateFrame()
        {
            if (IsRunning && Game1._Instance.IsActive)
            {
                // compute opCodes for 1 frame (456 T-Cycles per line @ 154 lines = 70.224)
                while (_cycleCount < 70224)
                {
                    // FETCH //
                    byte opCode = MMU.Read(CPU.PC);

                    // DECODE //
                    //  and
                    // EXECUTE //  
                    CPU.InstructionSet[opCode]();              // PC is pushed forward by instruction
                    if (CPU.PC == 0x100) MMU.IsBooting = false;

                    // UPDATE //
                    _cycleCount += CPU.InstructionCycles;
                    MMU.UpdateTimers(CPU.InstructionCycles);  // Timers
                    GPU.Update(CPU.InstructionCycles);        // GPU
                    APU.Update(CPU.InstructionCycles);        // SPU

                    // SYNC //
                    if (GPU.IsDrawTime && _cycleCount < 70224) _cycleCount = 70224 + GPU.ModeClock;   // exits loop when screen is drawn

                    // INTERRUPTS
                    checkInterrupts();

                    if (!IsRunning) break;
                }

                _cycleCount -= 70224;
            }
        }
        // ---

        // Private Helper Methods
        private void checkInterrupts()
        {
            checkInputRequest();

            // On HALT-Mode: Interrupt
            if (CPU.IsCPUHalt && (MMU.IE & MMU.IF) > 0)
            {
                // Exit HALT-Mode
                CPU.IsCPUHalt = false;
                // IME not set: Continue on next opCode without serving interrrupt
                if (!MMU.IME) CPU.PC++;                   // TODO: Implement HALT-Bug (Next Opcode handled twice)
                                                            // IME set: Continue with standard interrupt routine below
                if (MMU.IME) CPU.PC++;                    // incrementing for ISR not jumping back to HALT instruction
            }

            // Standard Interrupt Routine
            if (MMU.IME && (MMU.IE & MMU.IF) > 0)
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
            MMU.P1 |= 0b11001111;
            // INPUT interrupt
            if (MMU.P1[4] == 0 && MMU.P1[5] == 1)
            {
                if (IsButton_Right) { MMU.IF[4] = 1; MMU.P1[0] = 0; }
                if (IsButton_Left) { MMU.IF[4] = 1; MMU.P1[1] = 0; }
                if (IsButton_Up) { MMU.IF[4] = 1; MMU.P1[2] = 0; }
                if (IsButton_Down) { MMU.IF[4] = 1; MMU.P1[3] = 0; }
            }
            if (MMU.P1[4] == 1 && MMU.P1[5] == 0)
            {
                if (IsButton_A) { MMU.IF[4] = 1; MMU.P1[0] = 0; }
                if (IsButton_B) { MMU.IF[4] = 1; MMU.P1[1] = 0; }
                if (IsButton_Select) { MMU.IF[4] = 1; MMU.P1[2] = 0; }
                if (IsButton_Start) { MMU.IF[4] = 1; MMU.P1[3] = 0; }
            }
        }
        private void checkInterrupt(int index, ushort isrAddress)
        {
            if (MMU.IME &&
                MMU.IE[index] == 1 &&
                MMU.IF[index] == 1)
            {
                CPU.SP -= 2;                                   // save current position on stack
                MMU.WriteWord(CPU.SP, CPU.PC);
                CPU.PC = isrAddress;                           // set PC to ISR for next iteration
                CPU.InstructionCycles = 16;
                MMU.IME = false;                               // disable Interrupts
                MMU.IF[index] = 0;                             // reset Flag

                // UPDATE //
                _cycleCount += CPU.InstructionCycles;
                MMU.UpdateTimers(CPU.InstructionCycles);      // Timers
                GPU.Update(CPU.InstructionCycles);            // GPU
                APU.Update(CPU.InstructionCycles);            // SPU
            }
        }
        #endregion

    }
}
