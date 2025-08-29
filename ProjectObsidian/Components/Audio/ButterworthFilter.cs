using System;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;
using Awwdio;

namespace Obsidian.Components.Audio;

[Category(new string[] { "Obsidian/Audio/Filters" })]
public class ButterworthFilter : Component, Awwdio.IAudioDataSource, IWorldAudioDataSource, IWorldElement
{
    [Range(20f, 20000f, "0.00")]
    public readonly Sync<float> Frequency;

    [Range(0.1f, 1.41f, "0.00")]
    public readonly Sync<float> Resonance;

    public readonly Sync<bool> LowPass;

    public readonly SyncRef<IWorldAudioDataSource> Source;

    private ButterworthFilterController _controller = new();

    public bool IsActive
    {
        get
        {
            return Source.Target != null && Source.Target.IsActive;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        Frequency.Value = 20f;
        Resonance.Value = 1.41f;
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

            Span<S> span = stackalloc S[buffer.Length];

            span = buffer;

            Source.Target.Read(span, simulator);

            _controller.Process(span, simulator.SampleRate, LowPass, Frequency, Resonance);
        }
    }
}