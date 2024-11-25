using System;
using FrooxEngine;
using Elements.Assets;

namespace Obsidian.Components.Audio;

[Category(new string[] { "Audio" })]
public class EMA_IIR_SmoothSignal : Component, IAudioSource, IWorldElement
{
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> SmoothingFactor;

    public readonly SyncRef<IAudioSource> Source;

    public bool IsActive
    {
        get
        {
            return Source.Target != null && Source.Target.IsActive;
        }
    }

    public int ChannelCount => Source.Target?.ChannelCount ?? 0;

    public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
    {
        if (!IsActive)
        {
            return;
        }

        Span<S> span = stackalloc S[buffer.Length];

        span = buffer;

        Source.Target.Read(span);

        EMAIIRSmoothSignal(ref span, span.Length, SmoothingFactor);
    }

    // smoothingFactor is between 0.0 (no smoothing) and 0.9999.. (almost smoothing to DC) - *kind* of the inverse of cutoff frequency
    public void EMAIIRSmoothSignal<S>(ref Span<S> input, int N, float smoothingFactor = 0.8f) where S : unmanaged, IAudioSample<S>
    {
        // forward EMA IIR
        S acc = input[0];
        for (int i = 0; i < N; ++i)
        {
            acc = input[i].LerpTo(acc, smoothingFactor);
            input[i] = acc;
        }

        // backward EMA IIR - required only if we need to preserve the phase (aka make the filter symetric) - we usually want this
        acc = input[N - 1];
        for (int i = N - 1; i >= 0; --i)
        {
            acc = input[i].LerpTo(acc, smoothingFactor);
            input[i] = acc;
        }
    }
}