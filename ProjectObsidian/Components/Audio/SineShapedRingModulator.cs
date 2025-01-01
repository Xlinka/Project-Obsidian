using System;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;

namespace Obsidian.Components.Audio
{
    [Category(new string[] { "Obsidian/Audio" })]
    [OldTypeName("Obsidian.Components.Audio.FrequencyModulator")]
    public class SineShapedRingModulator : Component, IAudioSource, IWorldElement
    {
        [Range(0f, 5f, "0.00")]
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
            ModulationIndex.Value = 1f; // Default modulation index
        }

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            int channelCount = ChannelCount;
            if (channelCount == 0)
            {
                buffer.Fill(default(S));
                return;
            }

            // Temporary buffers for carrier and modulator sources
            Span<S> carrierBuffer = stackalloc S[buffer.Length];
            Span<S> modulatorBuffer = stackalloc S[buffer.Length];

            // Read data from sources
            CarrierSource.Target.Read(carrierBuffer);
            ModulatorSource.Target.Read(modulatorBuffer);

            float modulationIndex = ModulationIndex.Value;

            // Apply sine-shaped ring modulation
            for (int i = 0; i < buffer.Length; i++)
            {
                for (int j = 0; j < buffer[i].ChannelCount; j++)
                {
                    float carrierValue = carrierBuffer[i][j];
                    float modulatorValue = modulatorBuffer[i][j];

                    float modulatedValue = (float)(carrierValue * Math.Sin(2 * Math.PI * modulationIndex * modulatorValue));

                    buffer[i] = buffer[i].SetChannel(j, modulatedValue);

                    if (buffer[i][j] > 1f) buffer[i] = buffer[i].SetChannel(j, 1f);
                    if (buffer[i][j] < -1f) buffer[i] = buffer[i].SetChannel(j, -1f);
                }
            }
        }
    }
}