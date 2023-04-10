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
        int _frameSequencerTimer;
        int _frameSequencer;
        // ch 1
        int _ch1FreqTimer;
        int _ch1DutyIndex;
        int _ch1Amplitude;
        int _ch1SweepTimer;
        int _ch1ShadowFreq;
        bool _ch1SweepEnabled;
        int _ch1LengthTimer;
        int _ch1EnvelopeTimer;
        int _ch1CurrentVolume;
        // ch 2
        int _ch2FreqTimer;
        int _ch2DutyIndex;
        int _ch2Amplitude;
        int _ch2LengthTimer;
        int _ch2EnvelopeTimer;
        int _ch2CurrentVolume;

        Register[] _waveDutyTable;

        // sound effect
        int _sampleRate;
        int _bufferCount;
        float _sampleCycles;

        float _sampleTimer;
        DynamicSoundEffectInstance _soundEffectInstance;
        int _samplesPerBuffer;
        int _bufferSize;
        byte[] _buffer;
        int _bufferIndex;

        public float MasterVolume;
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

            // init audio
            _sampleRate = 48000;
            _soundEffectInstance = new DynamicSoundEffectInstance(_sampleRate, AudioChannels.Stereo);
            _samplesPerBuffer = (int)(_sampleRate / Game1.FRAME_RATE); // buffer size is 1 frame
            _bufferCount = 1; // target buffer count = latency
            _sampleCycles = 1f * Game1.CPU_FREQ / _sampleRate; // clock cycles for getting samples (~87)
            _bufferSize = _samplesPerBuffer * 2 * 2; // 2 = stereo, 2 = 2 byte per sample
            _buffer = new byte[_bufferSize];

            // subscribe trigger events
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
                _frameSequencer %= 8;

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
            // channel 1
            _ch1FreqTimer += instructionCycles;

            int ch1FreqCycles = (2048 - _mmu.CH1Frequency) * 4; 

            if (_ch1FreqTimer >= ch1FreqCycles)
            {
                _ch1FreqTimer -= ch1FreqCycles;

                _ch1DutyIndex++;
                _ch1DutyIndex %= 7;

                _ch1Amplitude = _waveDutyTable[_mmu.CH1WaveDuty][_ch1DutyIndex]; // amplitude is 0 or 1
            }

            // channel 2
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
        private void sweepClock()   // 128 Hz
        {
            // TODO: check sweep algorithm
            if (_ch1SweepTimer > 0)
            {
                _ch1SweepTimer--;
                if (_ch1SweepTimer == 0)
                {
                    _ch1SweepTimer = _mmu.CH1SweepTime;
                    if (_ch1SweepTimer == 0) _ch1SweepTimer = 8;
                    // frequency calculation
                    if (_ch1SweepEnabled && _mmu.CH1SweepTime > 0)
                    {
                        int newFreq = calculateFreq();
                        if (newFreq <= 2047 && _mmu.CH1SweepShifts > 0)
                        {
                            _mmu.CH1Frequency = newFreq;
                            _ch1ShadowFreq = newFreq;
                            // overflow check
                            calculateFreq();
                        }
                    }
                }
            }
        }
        private void envelopeClock() // 64 Hz
        {
            // channel 1
            if (_mmu.CH1EnvelopeTime != 0)
            {
                if (_ch1EnvelopeTimer > 0)
                {
                    _ch1EnvelopeTimer--;
                    if (_ch1EnvelopeTimer == 0)
                    {
                        _ch1EnvelopeTimer = _mmu.CH1EnvelopeTime;
                        switch (_mmu.CH1EnvelopeDirection)
                        {
                            case 0: // decrease
                                if (_ch1CurrentVolume > 0)
                                {
                                    _ch1CurrentVolume--;
                                }
                                break;
                            case 1: // increase
                                if (_ch1CurrentVolume < 15)
                                {
                                    _ch1CurrentVolume++;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // channel 2
            if (_mmu.CH2EnvelopeTime != 0)
            {
                if (_ch2EnvelopeTimer > 0)
                {
                    _ch2EnvelopeTimer--;
                    if (_ch2EnvelopeTimer == 0)
                    {
                        _ch2EnvelopeTimer = _mmu.CH2EnvelopeTime;
                        switch (_mmu.CH2EnvelopeDirection)
                        {
                            case 0: // decrease
                                if (_ch2CurrentVolume > 0)
                                {
                                    _ch2CurrentVolume--;
                                }
                                break;
                            case 1: // increase
                                if (_ch2CurrentVolume < 15)
                                {
                                    _ch2CurrentVolume++;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void updateSampler(int instructionCycles)
        {
            // get sample every ~87 clocks
            _sampleTimer += instructionCycles;
            if (_sampleTimer >= _sampleCycles)
            {
                _sampleTimer -= _sampleCycles;

                // get channel amplitudes
                float ch1AnalogOut = 0f;
                float ch2AnalogOut = 0f;
                if (_mmu.IsCH1On)
                {
                    int ch1DigitalOut =  _ch1Amplitude;             // range: 0 ... 1
                    ch1DigitalOut *= _ch1CurrentVolume;             // range: 0 ... 15
                    ch1AnalogOut = ch1DigitalOut / 15f * 2f - 1;    // range: -1 ... 1
                }
                if (_mmu.IsCH2On)
                {
                    int ch2DigitalOut =  _ch2Amplitude;             // range: 0 ... 1
                    ch2DigitalOut *= _ch2CurrentVolume;             // range: 0 ... 15
                    ch2AnalogOut = ch2DigitalOut / 15f * 2f - 1;    // range: -1 ... 1
                }

                // mixer left
                int leftCount = 0;
                float leftAnalog = 0;
                if (_mmu.IsCH1Left)
                {
                    leftAnalog += ch1AnalogOut;
                    leftCount++;
                }
                if (_mmu.IsCH2Left)
                {
                    leftAnalog += ch2AnalogOut;
                    leftCount++;
                }
                leftAnalog /= leftCount;

                // mixer right
                int rightCount = 0;
                float rightAnalog = 0;
                if (_mmu.IsCH1Right)
                {
                    rightAnalog += ch1AnalogOut;
                    rightCount++;
                }
                if (_mmu.IsCH2Right)
                {
                    rightAnalog += ch2AnalogOut;
                    rightCount++;
                }
                rightAnalog /= rightCount;

                // amplifier
                leftAnalog *= (_mmu.VolumeLeft + 1) / 8f;
                rightAnalog *= (_mmu.VolumeRight + 1) / 8f;

                // emulator volume
                leftAnalog *= MasterVolume;
                rightAnalog*= MasterVolume;

                // output
                short shortLeft = (short)(leftAnalog >= 0f ? leftAnalog * short.MaxValue : -1f * leftAnalog * short.MinValue);
                short shortRight = (short)(rightAnalog >= 0f ? rightAnalog * short.MaxValue : -1f * rightAnalog * short.MinValue);

                // fill buffer
                _buffer[_bufferIndex++] = (byte)shortLeft;
                _buffer[_bufferIndex++] = (byte)(shortLeft >> 8);
                _buffer[_bufferIndex++] = (byte)shortRight;
                _buffer[_bufferIndex++] = (byte)(shortRight >> 8);

                // submit buffer and play
                if (_bufferIndex >= _bufferSize)
                {
                    _soundEffectInstance.SubmitBuffer(_buffer);
                    if (_soundEffectInstance.PendingBufferCount >= _bufferCount)
                    {
                        _soundEffectInstance.Play();
                    }
                    _bufferIndex = 0;
                }
            }
        }

        private int calculateFreq()
        {
            // TODO: check sweep algorithm
            if (_mmu.CH1SweepShifts == 0) return 0;

            // calculate new sweep frequency
            int newFreq = _ch1ShadowFreq >> _mmu.CH1SweepShifts;
            switch (_mmu.CH1SweepDirection)
            {
                case 0: // increase
                    newFreq = _ch1ShadowFreq + newFreq;
                    break;
                case 1: // decrease
                    newFreq = _ch1ShadowFreq - newFreq;
                    break;
                default:
                    break;
            }
            if (newFreq > 2047)
            {
                _mmu.IsCH1On = false; // disable channel
            }
            return newFreq;
        }

        private void ch1TriggerHandler(Object sender, EventArgs e)
        {
            // initiate length timer
            _ch1LengthTimer = 64 - _mmu.CH1LengthData;
            // initial volume
            _ch1CurrentVolume = _mmu.CH1VolumeStart;
            _ch1EnvelopeTimer = _mmu.CH1EnvelopeTime;
            // sweep
            _ch1ShadowFreq = _mmu.CH1Frequency;
            _ch1SweepTimer = _mmu.CH1SweepTime;
            if (_ch1SweepTimer == 0) _ch1SweepTimer = 8;
            _ch1SweepEnabled = _mmu.CH1SweepTime != 0 || _mmu.CH1SweepShifts != 0;
            calculateFreq();
        }
        private void ch2TriggerHandler(Object sender, EventArgs e)
        {
            // initiate length timer
            _ch2LengthTimer = 64 - _mmu.CH2LengthData;
            // initial volume
            _ch2CurrentVolume = _mmu.CH2VolumeStart;
            _ch2EnvelopeTimer = _mmu.CH2EnvelopeTime;
        }

        #endregion
    }
}
