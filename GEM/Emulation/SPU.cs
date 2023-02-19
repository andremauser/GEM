using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace GEM.Emulation
{
    public class SPU
    {
        #region Fields
        const int SampleRate = 48000;
        const int ChannelsCount = 2;
        const int SamplesPerBuffer = (int)(SampleRate / (4194304f / 70224));
        const int BytesPerSample = 2;

        DynamicSoundEffectInstance _instance;
        float[,] _workingBuffer;
        int _workingBufferIndex;
        byte[] _xnaBuffer;

        MMU _mmu;
        int _sampleTimer;
        int _channel3Timer;
        int _channel3Pointer;
        int _channel3;
        #endregion

        #region Constructors
        public SPU(MMU mmu)
        {
            _mmu = mmu;
            _instance = new DynamicSoundEffectInstance(SampleRate, ChannelsCount == 2 ? AudioChannels.Stereo : AudioChannels.Mono);
            _workingBuffer = new float[ChannelsCount, SamplesPerBuffer];
            _xnaBuffer = new byte[ChannelsCount * SamplesPerBuffer * BytesPerSample];
            //_instance.Play();
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        public void Update(int instructionCycles)
        {
            updateChannel3(instructionCycles);

            updateSampler(instructionCycles);


        }

        private void updateChannel3(int instructionCycles)
        {
            _channel3Timer -= instructionCycles;
            if (_channel3Timer <= 0)
            {
                _channel3 = _mmu.GetChannel3WaveRamValue(_channel3Pointer); // get RAM value
                _channel3 >>= _mmu.Channel3VolumeShift;                    // apply volume
                _channel3Pointer++;                                         // move to next wave RAM position
                _channel3Pointer %= 32;
                _channel3Timer += 4194304 / _mmu.Channel3Frequency;         // reset timer
            }
        }

        private void updateSampler(int instructionCycles)
        {
            _sampleTimer -= instructionCycles;
            if (_sampleTimer <= 0)
            {
                _workingBuffer[0, _workingBufferIndex] = ((float)_channel3 / 15 - 0.5f) * 2;             // Left 
                _workingBuffer[1, _workingBufferIndex] = ((float)_channel3 / 15 - 0.5f) * 2;          // Right 
                _workingBufferIndex++;

                // When buffer is full -> submit
                if (_workingBufferIndex >= SamplesPerBuffer)
                {
                    ConvertBuffer(_workingBuffer, _xnaBuffer);
                    _instance.SubmitBuffer(_xnaBuffer);
                    _workingBufferIndex = 0;
                }
                _sampleTimer += 4194304 / SampleRate;   // reset timer
            }
        }


        private static void ConvertBuffer(float[,] source, byte[] destination)
        {
            int channels = source.GetLength(0);
            int samplesPerBuffer = source.GetLength(1);

            for (int i = 0; i < samplesPerBuffer; i++)
            {
                for (int c = 0; c < channels; c++)
                {
                    // First clamp the value to the [-1.0..1.0] range
                    float floatSample = MathHelper.Clamp(source[c, i], -1.0f, 1.0f);

                    // Convert it to the 16 bit [short.MinValue..short.MaxValue] range
                    short shortSample = (short)(floatSample >= 0.0f ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);

                    // Calculate the right index based on the PCM format of interleaved samples per channel [L-R-L-R]
                    int index = i * channels * BytesPerSample + c * BytesPerSample;

                    // Store the 16 bit sample as two consecutive 8 bit values in the buffer with regard to endian-ness
                    if (!BitConverter.IsLittleEndian)
                    {
                        destination[index] = (byte)(shortSample >> 8);
                        destination[index + 1] = (byte)shortSample;
                    }
                    else
                    {
                        destination[index] = (byte)shortSample;
                        destination[index + 1] = (byte)(shortSample >> 8);
                    }
                }
            }
        }


        #endregion
    }
}
