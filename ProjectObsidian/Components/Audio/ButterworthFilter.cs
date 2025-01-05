using System;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;

namespace Obsidian.Components.Audio;

[Category(new string[] { "Obsidian/Audio/Filters" })]
public class ButterworthFilter : Component, IAudioSource, IWorldElement
{
    [Range(20f, 20000f, "0.00")]
    public readonly Sync<float> Frequency;

    [Range(0.1f, 1.41f, "0.00")]
    public readonly Sync<float> Resonance;

    public readonly Sync<bool> LowPass;

    public readonly SyncRef<IAudioSource> Source;

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
            _controller.Clear();
        }
    }

    public int ChannelCount => Source.Target?.ChannelCount ?? 0;

    public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
    {
        if (!IsActive)
        {
            buffer.Fill(default(S));
            _controller.Clear();
            return;
        }

        Span<S> span = stackalloc S[buffer.Length];

        span = buffer;

        Source.Target.Read(span);

        _controller.Process(span, LowPass, Frequency, Resonance);
    }
}