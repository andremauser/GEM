using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GEM.Emulation
{
    public class APU
    {
        #region Fields
        MMU _mmu;
        int _cycleCounter = 0;
        int _frameSequencer = 0;
        Register[] _waveDutyTable;
        #endregion

        #region Constructors
        public APU(MMU mmu)
        {
            _mmu = mmu;
            _waveDutyTable = new Register[] {
                new Register(0b00000001),   // 12.5 %
                new Register(0b00000011),   // 25 %
                new Register(0b00001111),   // 50 %
                new Register(0b11111100)    // 75 %
            };
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public void Update(int instructionCycles)
        {
            _cycleCounter += instructionCycles;

            if (_cycleCounter >= 8192) // 512 Hz
            {
                frameSequencerStep();
                _cycleCounter -= 8192;
            }

        }

        private void frameSequencerStep()
        {
            _frameSequencer++;
            _frameSequencer %= 7;

            switch (_frameSequencer)
            {
                case 0:
                    lengthClock();
                    break;
                case 2:
                    lengthClock();
                    sweepClock();
                    break;
                case 4:
                    lengthClock();
                    break;
                case 6:
                    lengthClock();
                    sweepClock();
                    break;
                case 7:
                    envelopeClock();
                    break;
                default:
                    break;
            }
        }

        private void lengthClock()  // 256 Hz
        {

        }

        private void sweepClock()   // 128 Hz
        {

        }

        private void envelopeClock() // 64 Hz
        {

        }

        #endregion
    }
}
