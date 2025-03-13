using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using FrooxEngine;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using Elements.Assets;
using System;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    [ContinuouslyChanging]
    [NodeCategory("Obsidian/Audio")]
    public class LevelMonitor : ValueFunctionNode<FrooxEngineContext, float>
    {
        public readonly ObjectInput<IAudioSource> AudioInput;

        protected override float Compute(FrooxEngineContext context)
        {
            if (AudioInput.Evaluate(context) == null)
            {
                return default;
            }
            IAudioSource audio = AudioInput.Evaluate(context);

            switch (audio.ChannelCount)
            {
                case 1:
                    Span<MonoSample> buf = stackalloc MonoSample[Engine.Current.AudioSystem.SampleRate];
                    audio.Read(buf);
                    float sum = 0;
                    foreach (MonoSample sample in buf)
                    {
                        sum += sample.AbsoluteAmplitude;
                    }
                    return sum / buf.Length;
                case 2:
                    Span<StereoSample> buf2 = stackalloc StereoSample[Engine.Current.AudioSystem.SampleRate];
                    audio.Read(buf2);
                    float sum2 = 0;
                    foreach (MonoSample sample in buf2)
                    {
                        sum2 += sample.AbsoluteAmplitude;
                    }
                    return sum2 / buf2.Length;
                case 4:
                    Span<QuadSample> buf3 = stackalloc QuadSample[Engine.Current.AudioSystem.SampleRate];
                    audio.Read(buf3);
                    float sum3 = 0;
                    foreach (MonoSample sample in buf3)
                    {
                        sum3 += sample.AbsoluteAmplitude;
                    }
                    return sum3 / buf3.Length;
                case 6:
                    Span<Surround51Sample> buf4 = stackalloc Surround51Sample[Engine.Current.AudioSystem.SampleRate];
                    audio.Read(buf4);
                    float sum4 = 0;
                    foreach (MonoSample sample in buf4)
                    {
                        sum4 += sample.AbsoluteAmplitude;
                    }
                    return sum4 / buf4.Length;
            }

            return 0;
        }
    }
}