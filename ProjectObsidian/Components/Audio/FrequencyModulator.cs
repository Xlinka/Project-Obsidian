using System;
using FrooxEngine;
using Elements.Assets;

namespace Obsidian.Components.Audio
{
    [Category(new string[] { "Obsidian/Audio" })]
    public class FrequencyModulator : Component, IAudioSource, IWorldElement
    {
        [Range(0f, 1000f, "0.00")]
        public readonly Sync<float> ModulationIndex;

        public readonly SyncRef<IAudioSource> CarrierSource;
        public readonly SyncRef<IAudioSource> ModulatorSource;

        public bool IsActive
        {
            get
            {
                return CarrierSource.Target != null &&
                       ModulatorSource.Target != null &&
                       CarrierSource.Target.IsActive &&
                       ModulatorSource.Target.IsActive;
            }
        }

        public int ChannelCount
        {
            get
            {
                return CarrierSource.Target?.ChannelCount ?? 0;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            ModulationIndex.Value = 100f; // Default modulation index
        }

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                return;
            }

            int channelCount = ChannelCount;
            if (channelCount == 0)
            {
                return;
            }

            // Temporary buffers for carrier and modulator sources
            Span<S> carrierBuffer = stackalloc S[buffer.Length];
            Span<S> modulatorBuffer = stackalloc S[buffer.Length];

            // Read data from sources
            CarrierSource.Target.Read(carrierBuffer);
            ModulatorSource.Target.Read(modulatorBuffer);

            float modulationIndex = ModulationIndex.Value;

            // Apply FM synthesis
            for (int i = 0; i < buffer.Length; i++)
            {
                // Get carrier and modulator values
                float carrierValue = carrierBuffer[i].AbsoluteAmplitude;
                float modulatorValue = modulatorBuffer[i].AbsoluteAmplitude;

                // Compute frequency modulation
                float modulatedValue = (float)(carrierValue * Math.Sin(2 * Math.PI * modulationIndex * modulatorValue));

                // Write modulated value to the buffer
                buffer[i] = buffer[i].Bias(buffer[i].AbsoluteAmplitude -  modulatedValue);
            }
        }
    }
}