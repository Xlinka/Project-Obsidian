using System;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;
using Elements.Core;
using Awwdio;

namespace Obsidian.Components.Audio
{
    [Category(new string[] { "Obsidian/Audio/Effects" })]
    public class PhaseModulator : Component, Awwdio.IAudioDataSource, IWorldAudioDataSource, IWorldElement
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

        // TODO: Make this not click when the signal goes silent and then not silent?
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

            Span<S> carrierBuffer = stackalloc S[buffer.Length];
            Span<S> modulatorBuffer = stackalloc S[buffer.Length];

            CarrierSource.Target.Read(carrierBuffer, simulator);
            ModulatorSource.Target.Read(modulatorBuffer, simulator);

            float modulationIndex = ModulationIndex.Value;

            Algorithms.PhaseModulation(buffer, carrierBuffer, modulatorBuffer, modulationIndex, channelCount);
        }
    }
}