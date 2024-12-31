using System;
using FrooxEngine;
using Elements.Assets;
using System.Collections.Generic;

namespace Obsidian.Components.Audio;

[Category(new string[] { "Obsidian/Audio" })]
public class ButterworthFilter : Component, IAudioSource, IWorldElement
{
    [Range(20f, 20000f, "0.00")]
    public readonly Sync<float> Frequency;

    [Range(0.1f, 1.41f, "0.00")]
    public readonly Sync<float> Resonance;

    public readonly Sync<bool> LowPass;

    public readonly SyncRef<IAudioSource> Source;

    private double lastTime;

    private Dictionary<Type, object> filters = new();

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
        lastTime = Engine.Current.AudioSystem.DSPTime;
        Frequency.Value = 20f;
        Resonance.Value = 1.41f;
    }

    protected override void OnChanges()
    {
        base.OnChanges();
        if (Source.GetWasChangedAndClear())
        {
            filters.Clear();
        }
    }

    public int ChannelCount => Source.Target?.ChannelCount ?? 0;

    public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
    {
        if (!IsActive)
        {
            buffer.Fill(default(S));
            filters.Clear();
            return;
        }

        Span<S> span = stackalloc S[buffer.Length];

        span = buffer;

        Source.Target.Read(span);

        if (!filters.TryGetValue(typeof(S), out object filter)) {
            filter = new FilterButterworth<S>();
            filters.Add(typeof(S), filter);
        }

        ((FilterButterworth<S>)filter).UpdateCoefficients(Frequency, Engine.AudioSystem.SampleRate, LowPass ? FilterButterworth<S>.PassType.Lowpass : FilterButterworth<S>.PassType.Highpass, Resonance);

        for (int i = 0; i < span.Length; i++)
        {
            ((FilterButterworth<S>)filter).Update(ref span[i]);
        }
    }

    public class FilterButterworth<S> where S: unmanaged, IAudioSample<S>
    {
        /// <summary>
        /// rez amount, from sqrt(2) to ~ 0.1
        /// </summary>
        private readonly float resonance;

        private readonly float frequency;
        private readonly int sampleRate;
        private readonly PassType passType;

        private float c, a1, a2, a3, b1, b2;

        /// <summary>
        /// Array of input values, latest are in front
        /// </summary>
        private S[] inputHistory = new S[2];

        /// <summary>
        /// Array of output values, latest are in front
        /// </summary>
        private S[] outputHistory = new S[3];

        public void UpdateCoefficients(float frequency, int sampleRate, PassType passType, float resonance)
        {
            switch (passType)
            {
                case PassType.Lowpass:
                    c = 1.0f / (float)Math.Tan(Math.PI * frequency / sampleRate);
                    a1 = 1.0f / (1.0f + resonance * c + c * c);
                    a2 = 2f * a1;
                    a3 = a1;
                    b1 = 2.0f * (1.0f - c * c) * a1;
                    b2 = (1.0f - resonance * c + c * c) * a1;
                    break;
                case PassType.Highpass:
                    c = (float)Math.Tan(Math.PI * frequency / sampleRate);
                    a1 = 1.0f / (1.0f + resonance * c + c * c);
                    a2 = -2f * a1;
                    a3 = a1;
                    b1 = 2.0f * (c * c - 1.0f) * a1;
                    b2 = (1.0f - resonance * c + c * c) * a1;
                    break;
            }
        }

        public enum PassType
        {
            Highpass,
            Lowpass,
        }

        public void Update(ref S newInput)
        {
            S first = newInput.Multiply(a1);
            S second = this.inputHistory[0].Multiply(a2);
            S third = this.inputHistory[1].Multiply(a3);
            S fourth = this.outputHistory[0].Multiply(b1);
            S fifth = this.outputHistory[1].Multiply(b2);
            S final = first.Add(second).Add(third).Subtract(fourth).Subtract(fifth);

            for (int i = 0; i < final.ChannelCount; i++)
            {
                if (final[i] > 1f) final = final.SetChannel(i, 1f);
                else if (final[i] < -1f) final = final.SetChannel(i, -1f);
            }

            this.inputHistory[1] = this.inputHistory[0];
            this.inputHistory[0] = newInput;

            this.outputHistory[2] = this.outputHistory[1];
            this.outputHistory[1] = this.outputHistory[0];
            this.outputHistory[0] = final;

            newInput = final;
        }

        public S Value
        {
            get { return this.outputHistory[0]; }
        }
    }
}