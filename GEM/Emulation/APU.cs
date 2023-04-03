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
        int _frameSequencerCounter = 0;
        int _frameSequencer = 0;
        int _ch1CycleCounter = 0;
        int _ch1DutyStep = 0;
        int _ch1Amplitude;
        int _ch2CycleCounter = 0;
        int _ch2DutyStep = 0;
        int _ch2Amplitude;
        Register[] _waveDutyTable;
        #endregion

        #region Constructors
        public APU(MMU mmu)
        {
            _mmu = mmu;
            _waveDutyTable = new Register[] {
                new Register(0b10000000),   // 12.5 %
                new Register(0b11000000),   // 25 %
                new Register(0b11110000),   // 50 %
                new Register(0b00111111)    // 75 %
            };
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public void Update(int instructionCycles)
        {
            updateFrameSequencer(instructionCycles);

            updatePulseAmplitudes(instructionCycles);
            

        }

        private void updateFrameSequencer(int instructionCycles)
        {
            // clock sweep, envelope and length
            _frameSequencerCounter += instructionCycles;

            if (_frameSequencerCounter >= 8192) // 512 Hz
            {
                _frameSequencerCounter -= 8192;
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
        }
        private void updatePulseAmplitudes(int instructionCycles)
        {
            // CH1
            _ch1CycleCounter += instructionCycles;

            int ch1StepCycles = (2048 - _mmu.CH1Frequency) * 4;

            if (_ch1CycleCounter >= ch1StepCycles)
            {
                _ch1CycleCounter -= ch1StepCycles;

                _ch1DutyStep++;
                _ch1DutyStep %= 7;

                _ch1Amplitude = _waveDutyTable[_mmu.CH1WaveDuty][_ch1DutyStep]; // amplitude is 0 or 1
            }

            // CH2
            _ch2CycleCounter += instructionCycles;

            int ch2StepCycles = (2048 - _mmu.CH2Frequency) * 4;

            if (_ch2CycleCounter >= ch2StepCycles)
            {
                _ch2CycleCounter -= ch2StepCycles;

                _ch2DutyStep++;
                _ch2DutyStep %= 7;

                _ch2Amplitude = _waveDutyTable[_mmu.CH2WaveDuty][_ch2DutyStep];  // amplitude is 0 or 1
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
