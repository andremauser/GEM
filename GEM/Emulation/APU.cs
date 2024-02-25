using Microsoft.Xna.Framework.Audio;
using System;

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
        // ch 2
        int _ch2FreqTimer;
        int _ch2DutyIndex;
        int _ch2Amplitude;
        int _ch2LengthTimer;
        int _ch2EnvelopeTimer;
        // ch 3
        int _ch3FreqTimer;
        int _ch3LengthTimer;
        int _ch3SampleIndex;
        int _ch3Amplitude;
        // ch 4
        int _ch4FreqTimer;
        int _ch4LengthTimer;
        int _ch4Amplitude;
        int _ch4EnvelopeTimer;
        int _ch4LFSR;

        float[] _channelVolume = { 0, 0, 0, 0};

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
            _mmu.CH3TriggerEvent += ch3TriggerHandler;
            _mmu.CH4TriggerEvent += ch4TriggerHandler;
        }
        #endregion

        #region Properties
        public float  MasterVolume { get; set; }
        public bool[] MasterSwitch { get; set; } = { true, true, true, true }; // channel 1-4 master switch
        public bool[] IsChannelOn
        {
            get
            {
                return new bool[] { _mmu.IsCH1On, _mmu.IsCH2On, _mmu.IsCH3On, _mmu.IsCH4On };
            }
        }
        public bool[] IsChannelOutput
        {
            get
            {
                return new bool[] { _mmu.IsCH1On && (_channelVolume[0] > 0),
                                    _mmu.IsCH2On && (_channelVolume[1] > 0), 
                                    _mmu.IsCH3On && (_channelVolume[2] > 0), 
                                    _mmu.IsCH4On && (_channelVolume[3] > 0) };
            }
        }
        #endregion

        #region Methods
        // ---
        public void Update(int instructionCycles)
        {
            updateFrameSequencer(instructionCycles);
            updateChannelAmplitudes(instructionCycles);
            updateSampler(instructionCycles);
        }
        // ---

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
        private void updateChannelAmplitudes(int instructionCycles)
        {
            // channel 1
            _ch1FreqTimer += instructionCycles;
            int ch1FreqCycles = (2048 - _mmu.CH1Frequency) * 4; 
            if (_ch1FreqTimer >= ch1FreqCycles)
            {
                // reset timer
                _ch1FreqTimer -= ch1FreqCycles;
                // move to next duty step
                _ch1DutyIndex++;
                _ch1DutyIndex %= 8;
                // get amplitude
                _ch1Amplitude = _waveDutyTable[_mmu.CH1WaveDuty][_ch1DutyIndex]; // amplitude is 0 or 1
            }

            // channel 2
            _ch2FreqTimer += instructionCycles;
            int ch2FreqCycles = (2048 - _mmu.CH2Frequency) * 4;
            if (_ch2FreqTimer >= ch2FreqCycles)
            {
                _ch2FreqTimer -= ch2FreqCycles;
                _ch2DutyIndex++;
                _ch2DutyIndex %= 8;
                _ch2Amplitude = _waveDutyTable[_mmu.CH2WaveDuty][_ch2DutyIndex];  // amplitude is 0 or 1
            }

            // channel 3
            _ch3FreqTimer += instructionCycles;
            int ch3FreqCycles = (2048 - _mmu.CH3Frequency) * 2;
            if (_ch3FreqTimer >= ch3FreqCycles)
            {
                _ch3FreqTimer -= ch3FreqCycles;
                _ch3SampleIndex++;
                _ch3SampleIndex %= 32;
                _ch3Amplitude = _mmu.GetChannel3WaveRamValue(_ch3SampleIndex);  // amplitude is 0 ... 15
            }

            // channel 4
            _ch4FreqTimer -= instructionCycles;
            if (_ch4FreqTimer <= 0)
            {
                int s = _mmu.CH4ClockShift;
                int r = _mmu.CH4ClockDivider;
                // set new frequency
                _ch4FreqTimer = (r > 0 ? (r << 4) : 8) << s;
                int xorResult = (_ch4LFSR & 0b01) ^ ((_ch4LFSR & 0b10) >> 1);
                // shift right and put result in bit 14
                _ch4LFSR = (_ch4LFSR >> 1) | (xorResult << 14);
                // if width bit is set, also put in bit 6
                if (_mmu.CH4WidthMode == 1) 
                {
                    _ch4LFSR &= 0b111111;
                    _ch4LFSR |= xorResult << 6;
                }
                // amplitude is inverted bit 0
                if ((_ch4LFSR & 1) == 0)
                {
                    _ch4Amplitude = 1;
                }
                else
                {
                    _ch4Amplitude = 0;
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
                // channel 1
                float ch1AnalogOut = 0f;
                if (_mmu.IsCH1On)
                {
                    int ch1DigitalOut = _ch1Amplitude;             // range: 0 ... 1
                    ch1DigitalOut *= (int)_channelVolume[0];        // range: 0 ... 15
                    ch1AnalogOut = ch1DigitalOut / 15f * 2f - 1;    // range: -1 ... 1
                }
                // channel 2
                float ch2AnalogOut = 0f;
                if (_mmu.IsCH2On)
                {
                    int ch2DigitalOut = _ch2Amplitude;             // range: 0 ... 1
                    ch2DigitalOut *= (int)_channelVolume[1];        // range: 0 ... 15
                    ch2AnalogOut = ch2DigitalOut / 15f * 2f - 1;    // range: -1 ... 1
                }
                // channel 3
                float ch3AnalogOut = 0f;
                if (_mmu.IsCH3On)
                {
                    int ch3DigitalOut = _ch3Amplitude;              // range: 0 ... 15
                    _channelVolume[2] = 0;
                    switch (_mmu.CH3VolumeSelect)
                    {
                        case 1:
                            _channelVolume[2] = 1f;
                            break;
                        case 2:
                            _channelVolume[2] = 0.5f; ;
                            break;
                        case 3:
                            _channelVolume[2] = 0.25f;
                            break;
                    }
                    ch3DigitalOut = (int)(ch3DigitalOut * _channelVolume[2]);
                    ch3AnalogOut = ch3DigitalOut / 15f * 2f - 1;    // range: -1 ... 1
                }
                // channel 4
                float ch4AnalogOut = 0f;
                if (_mmu.IsCH4On)
                {
                    int ch4DigitalOut = _ch4Amplitude;              // range: 0 ... 1
                    ch4DigitalOut *= (int)_channelVolume[3];        // range: 0 ... 15
                    ch4AnalogOut = ch4DigitalOut / 15f * 2f - 1;    // range: -1 ... 1
                }

                // mixer left
                float leftAnalog = 0;
                if (_mmu.IsCH1Left && MasterSwitch[0])
                {
                    leftAnalog += ch1AnalogOut;
                }
                if (_mmu.IsCH2Left && MasterSwitch[1])
                {
                    leftAnalog += ch2AnalogOut;
                }
                if (_mmu.IsCH3Left && MasterSwitch[2])
                {
                    leftAnalog += ch3AnalogOut;
                }
                if (_mmu.IsCH4Left && MasterSwitch[3])
                {
                    leftAnalog += ch4AnalogOut;
                }
                leftAnalog /= 4;

                // mixer right
                float rightAnalog = 0;
                if (_mmu.IsCH1Right && MasterSwitch[0])
                {
                    rightAnalog += ch1AnalogOut;
                }
                if (_mmu.IsCH2Right && MasterSwitch[1])
                {
                    rightAnalog += ch2AnalogOut;
                }
                if (_mmu.IsCH3Right && MasterSwitch[2])
                {
                    rightAnalog += ch3AnalogOut;
                }
                if (_mmu.IsCH4Right && MasterSwitch[3])
                {
                    rightAnalog += ch4AnalogOut;
                }
                rightAnalog /= 4;

                // amplifier
                leftAnalog *= (_mmu.VolumeLeft + 1) / 8f;
                rightAnalog *= (_mmu.VolumeRight + 1) / 8f;

                // emulator volume
                leftAnalog *= MasterVolume;
                rightAnalog *= MasterVolume;

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

            if (_mmu.IsCH3LengthEnabled)
            {
                _ch3LengthTimer--;
                if (_ch3LengthTimer == 0)
                {
                    _mmu.IsCH3On = false;
                }
            }

            if (_mmu.IsCH4LengthEnabled)
            {
                _ch4LengthTimer--;
                if (_ch4LengthTimer == 0)
                {
                    _mmu.IsCH4On = false;
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
                        int newFreq = calcSweepFreq();
                        if (newFreq <= 2047 && _mmu.CH1SweepShifts > 0)
                        {
                            _mmu.CH1Frequency = newFreq;
                            _ch1ShadowFreq = newFreq;
                            // overflow check
                            calcSweepFreq();
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
                                if (_channelVolume[0] > 0)
                                {
                                    _channelVolume[0]--;
                                }
                                break;
                            case 1: // increase
                                if (_channelVolume[0] < 15)
                                {
                                    _channelVolume[0]++;
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
                                if (_channelVolume[1] > 0)
                                {
                                    _channelVolume[1]--;
                                }
                                break;
                            case 1: // increase
                                if (_channelVolume[1] < 15)
                                {
                                    _channelVolume[1]++;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // channel 4
            if (_mmu.CH4EnvelopeTime != 0)
            {
                if (_ch4EnvelopeTimer > 0)
                {
                    _ch4EnvelopeTimer--;
                    if (_ch4EnvelopeTimer == 0)
                    {
                        _ch4EnvelopeTimer = _mmu.CH4EnvelopeTime;
                        switch (_mmu.CH4EnvelopeDirection)
                        {
                            case 0: // decrease
                                if (_channelVolume[3] > 0)
                                {
                                    _channelVolume[3]--;
                                }
                                break;
                            case 1: // increase
                                if (_channelVolume[3] < 15)
                                {
                                    _channelVolume[3]++;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private int calcSweepFreq()
        {
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
            _channelVolume[0] = _mmu.CH1VolumeStart;
            _ch1EnvelopeTimer = _mmu.CH1EnvelopeTime;
            // sweep
            _ch1ShadowFreq = _mmu.CH1Frequency;
            _ch1SweepTimer = _mmu.CH1SweepTime;
            if (_ch1SweepTimer == 0) _ch1SweepTimer = 8;
            _ch1SweepEnabled = _mmu.CH1SweepTime != 0 || _mmu.CH1SweepShifts != 0;
            calcSweepFreq();
        }
        private void ch2TriggerHandler(Object sender, EventArgs e)
        {
            // initiate length timer
            _ch2LengthTimer = 64 - _mmu.CH2LengthData;
            // initial volume
            _channelVolume[1] = _mmu.CH2VolumeStart;
            _ch2EnvelopeTimer = _mmu.CH2EnvelopeTime;
        }
        private void ch3TriggerHandler(Object sender, EventArgs e)
        {
            // initiate length timer
            _ch3LengthTimer = 256 - _mmu.CH3LengthData;
        }
        private void ch4TriggerHandler(Object sender, EventArgs e)
        {
            // initiate length timer
            _ch4LengthTimer = 64 - _mmu.CH4LengthData;
            // initial volume
            _channelVolume[3] = _mmu.CH4VolumeStart;
            _ch4EnvelopeTimer = _mmu.CH4EnvelopeTime;
            // set LFSR
            _ch4LFSR = 0xFFFF; // all bits set to 1
        }

        #endregion
    }
}
