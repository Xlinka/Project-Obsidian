using System;
using FrooxEngine;
using Elements.Assets;

namespace Obsidian.Components.Audio;

[Category(new string[] { "Audio" })]
public class AudioLowPassFilter : Component, IAudioSource, IWorldElement
{
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> SmoothingFactor;

    public readonly AssetRef<AudioClip> Clip;

    private double lastPos;

    private double nextPos;

    private double lastAudioTime;

    public bool IsActive
    {
        get
        {
            return Clip.IsAssetAvailable;
        }
    }

    public int ChannelCount => (Clip.Asset?.Data?.ChannelCount).GetValueOrDefault();

    public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
    {
        var clipData = Clip.Asset?.Data;
        if (clipData == null)
        {
            buffer.Fill(default(S));
            return;
        }
        double dSPTime = base.Engine.AudioSystem.DSPTime;
        if (lastAudioTime != dSPTime)
        {
            lastPos = nextPos;
            lastAudioTime = dSPTime;
        }
        bool flag = false;
        Span<S> span = stackalloc S[buffer.Length];
        if (!IsActive)
        {
            return;
        }
        Span<S> span2 = span;
        if (!flag)
        {
            span2 = buffer;
        }

        int num = Clip.Asset.Data.Read(span2, lastPos * (double)Clip.Asset.Data.SampleRate, 1f, loop: true);

        EMAIIRSmoothSignal(ref span2, span2.Length, SmoothingFactor);

        nextPos = (lastPos + (double)num * base.Engine.AudioSystem.InvSampleRate * (double)1f) % Clip.Asset.Data.Duration;

        if (flag)
        {
            buffer.Add(span2);
        }
        flag = true;
        if (!flag)
        {
            buffer.Fill(default(S));
        }
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