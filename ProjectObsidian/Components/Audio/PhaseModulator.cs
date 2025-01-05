using System;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;

namespace Obsidian.Components.Audio
{
    [Category(new string[] { "Obsidian/Audio/Effects" })]
    public class PhaseModulator : Component, IAudioSource, IWorldElement
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

        // TODO: Make this not click when the signal goes silent and then not silent?
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

            Span<S> carrierBuffer = stackalloc S[buffer.Length];
            Span<S> modulatorBuffer = stackalloc S[buffer.Length];

            CarrierSource.Target.Read(carrierBuffer);
            ModulatorSource.Target.Read(modulatorBuffer);

            float modulationIndex = ModulationIndex.Value;

            Algorithms.PhaseModulation(buffer, carrierBuffer, modulatorBuffer, modulationIndex);
        }
    }
}