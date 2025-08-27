using System;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using Obsidian.Elements;
using Awwdio;
using Elements.Data;

namespace Obsidian.Components.Audio
{
    [Category(new string[] { "Obsidian/Audio/Effects" })]
    [OldTypeName("Obsidian.Components.Audio.FrequencyModulator")]
    public class SineShapedRingModulator : Component, Awwdio.IAudioDataSource, IWorldAudioDataSource, IWorldElement
    {
        [Range(0f, 5f, "0.00")]
        public readonly Sync<float> ModulationIndex;

        public readonly SyncRef<IWorldAudioDataSource> CarrierSource;
        public readonly SyncRef<IWorldAudioDataSource> ModulatorSource;

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
                return MathX.Min(CarrierSource.Target?.ChannelCount ?? 0, ModulatorSource.Target?.ChannelCount ?? 0);
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            ModulationIndex.Value = 1f; // Default modulation index
        }

        public void Read<S>(Span<S> buffer, AudioSimulator simulator) where S : unmanaged, IAudioSample<S>
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
            CarrierSource.Target.Read(carrierBuffer, simulator);
            ModulatorSource.Target.Read(modulatorBuffer, simulator);

            float modulationIndex = ModulationIndex.Value;

            Algorithms.SineShapedRingModulation(buffer, carrierBuffer, modulatorBuffer, modulationIndex, channelCount);
        }
    }
}