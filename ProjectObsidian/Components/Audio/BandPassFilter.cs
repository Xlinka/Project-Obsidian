using System;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;
using Awwdio;

namespace Obsidian.Components.Audio
{
    [Category(new string[] { "Obsidian/Audio/Filters" })]
    public class BandPassFilter : Component, Awwdio.IAudioDataSource, IWorldAudioDataSource, IWorldElement
    {
        [Range(0.1f, 1.41f, "0.00")]
        public readonly Sync<float> Resonance;

        [Range(20f, 20000f, "0.00")]
        public readonly Sync<float> LowFrequency;

        [Range(20f, 20000f, "0.00")]
        public readonly Sync<float> HighFrequency;

        public readonly SyncRef<IWorldAudioDataSource> Source;

        private BandPassFilterController _controller = new();

        public bool IsActive
        {
            get => Source.Target != null && Source.Target.IsActive;
        }

        public int ChannelCount => Source.Target?.ChannelCount ?? 0;

        public void Read<S>(Span<S> buffer, AudioSimulator simulator) where S : unmanaged, IAudioSample<S>
        {
            lock (_controller)
            {
                if (Source.Target == null)
                {
                    _controller.Clear();
                }
                if (!IsActive || Source.Target == null)
                {
                    buffer.Fill(default(S));
                    return;
                }

                Span<S> tempBuffer = stackalloc S[buffer.Length];
                tempBuffer = buffer;

                Source.Target.Read(tempBuffer, simulator);

                _controller.Process(tempBuffer, simulator.SampleRate, LowFrequency, HighFrequency, Resonance);
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            Resonance.Value = 1.41f;
            LowFrequency.Value = 20f;
            HighFrequency.Value = 20000f;
        }

        protected override void OnChanges()
        {
            base.OnChanges();
            if (Source.GetWasChangedAndClear())
            {
                lock (_controller)
                    _controller.Clear();
            }
        }
    }
}