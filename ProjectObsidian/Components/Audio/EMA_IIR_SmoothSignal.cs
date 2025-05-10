using System;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;
using Awwdio;

namespace Obsidian.Components.Audio;

[Category(new string[] { "Obsidian/Audio/Filters" })]
public class EMA_IIR_SmoothSignal : Component, Awwdio.IAudioDataSource, IWorldAudioDataSource, IWorldElement
{
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> SmoothingFactor;

    public readonly SyncRef<IWorldAudioDataSource> Source;

    public bool IsActive
    {
        get
        {
            return Source.Target != null && Source.Target.IsActive;
        }
    }

    public int ChannelCount => Source.Target?.ChannelCount ?? 0;

    public void Read<S>(Span<S> buffer, AudioSimulator simulator) where S : unmanaged, IAudioSample<S>
    {
        if (!IsActive)
        {
            buffer.Fill(default(S));
            return;
        }

        Span<S> span = stackalloc S[buffer.Length];

        span = buffer;

        Source.Target.Read(span, simulator);

        Algorithms.EMAIIRSmoothSignal(ref span, span.Length, SmoothingFactor);
    }
}