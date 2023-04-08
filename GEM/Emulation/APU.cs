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
        // frame sequencer
        int _frameSequencerTimer = 0;
        int _frameSequencer = 0;
        // ch 1
        int _ch1FreqTimer;
        int _ch1DutyIndex;
        int _ch1Amplitude;
        int _ch1SweepTimer;
        int _ch1LengthTimer;
        // ch 2
        int _ch2FreqTimer;
        int _ch2DutyIndex;
        int _ch2Amplitude;
        int _ch2LengthTimer;

        Register[] _waveDutyTable;

        // sound effect
        const int SAMPLE_RATE = 48000;
        const int FRAME_LATENCY = 1;

        float _sampleTimer;
        DynamicSoundEffectInstance _soundEffectInstance;
        int _samplesPerBuffer;
        int _bufferSize;
        byte[] _buffer;
        int _bufferIndex;
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
            _soundEffectInstance = new DynamicSoundEffectInstance(SAMPLE_RATE, AudioChannels.Stereo);
            _samplesPerBuffer = (int)(SAMPLE_RATE / Game1.FRAME_RATE); // buffer is 1 frame
            _bufferSize = _samplesPerBuffer * 2 * 2; // 2 = stereo, 2 = 2 byte per sample
            _buffer = new byte[_bufferSize];
            _mmu.CH1TriggerEvent += ch1TriggerHandler;
            _mmu.CH2TriggerEvent += ch2TriggerHandler;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public void Update(int instructionCycles)
        {
            // update sweep, envelope and length
            updateFrameSequencer(instructionCycles);
            // update channel 1 and channel 2 amplitdes
            updatePulseAmplitudes(instructionCycles);
            // TODO: update channel 3
            // TODO: update channel 4
            // TODO: mixer
            // update sampler
            updateSampler(instructionCycles);
        }

        private void updateFrameSequencer(int instructionCycles)
        {
            // clock sweep, envelope and length
            _frameSequencerTimer += instructionCycles;

            if (_frameSequencerTimer >= 8192) // 512 Hz
            {
                _frameSequencerTimer -= 8192;
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
            _ch1FreqTimer += instructionCycles;

            int ch1FreqCycles = (2048 - _mmu.CH1Frequency) * 4;

            if (_ch1FreqTimer >= ch1FreqCycles)
            {
                _ch1FreqTimer -= ch1FreqCycles;

                _ch1DutyIndex++;
                _ch1DutyIndex %= 7;

                _ch1Amplitude = _waveDutyTable[_mmu.CH1WaveDuty][_ch1DutyIndex]; // amplitude is 0 or 1
            }

            // CH2
            _ch2FreqTimer += instructionCycles;

            int ch2FreqCycles = (2048 - _mmu.CH2Frequency) * 4;

            if (_ch2FreqTimer >= ch2FreqCycles)
            {
                _ch2FreqTimer -= ch2FreqCycles;

                _ch2DutyIndex++;
                _ch2DutyIndex %= 7;

                _ch2Amplitude = _waveDutyTable[_mmu.CH2WaveDuty][_ch2DutyIndex];  // amplitude is 0 or 1
            }
        }

        private void lengthClock()  // 256 Hz
        {
            if (_mmu.IsCH1LengthEnabled)
            {
                _ch1LengthTimer--;
                if (_ch1LengthTimer == 0)
                {
                    _mmu.IsCH1On = false;
                }
            }

            if (_mmu.IsCH2LengthEnabled)
            {
                _ch2LengthTimer--;
                if (_ch2LengthTimer == 0)
                {
                    _mmu.IsCH2On = false;
                }
            }
        }

        private void ch1TriggerHandler(Object sender, EventArgs e)
        {
            // initiate length timer
            _ch1LengthTimer = 64 - _mmu.CH1LengthData;
        }
        private void ch2TriggerHandler(Object sender, EventArgs e)
        {
            // initiate length timer
            _ch2LengthTimer = 64 - _mmu.CH2LengthData;
        }

        private void sweepClock()   // 128 Hz
        {
            _ch1SweepTimer++;

            if (_ch1SweepTimer >= _mmu.CH1SweepTime)
            {
                _ch1SweepTimer = 0;
            }
            

        }

        private void envelopeClock() // 64 Hz
        {

        }

        private void updateSampler(int instructionCycles)
        {
            _sampleTimer += instructionCycles;
            float sampleCycles = 1f * Game1.CPU_FREQ / SAMPLE_RATE;

            if (_sampleTimer >= sampleCycles)
            {
                _sampleTimer -= sampleCycles;

                // fill buffer (quick and dirty)
                if ((_bufferIndex + 4) >= _bufferSize)
                {
                    _soundEffectInstance.SubmitBuffer(_buffer);
                    if (_soundEffectInstance.PendingBufferCount >= FRAME_LATENCY)
                    {
                        _soundEffectInstance.Play();
                    }
                    _bufferIndex = 0;
                }
                int ch1 = 0;
                int ch2 = 0;
                if (_mmu.IsCH1On)
                {
                    ch1 = _ch1Amplitude; // range:  0 ... 1 TODO: Mixer etc..
                }
                if (_mmu.IsCH2On)
                {
                    ch2 = _ch2Amplitude; // range:  0 ... 1 TODO: Mixer etc..
                }
                float sample = (ch1 + ch2) / 2f;
                float floatSample = 2f * sample - 1;     // range: -1 ... 1
                short shortSample = (short)(floatSample >= 0f ? floatSample * short.MaxValue : -1f * floatSample * short.MinValue);

                _buffer[_bufferIndex + 0] = (byte)shortSample;
                _buffer[_bufferIndex + 1] = (byte)(shortSample >> 8);
                _buffer[_bufferIndex + 2] = (byte)shortSample;
                _buffer[_bufferIndex + 3] = (byte)(shortSample >> 8);
                _bufferIndex += 4;
            }
        }

        #endregion
    }
}
