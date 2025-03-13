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

        private float lastValue = 0f;

        private bool subscribed = false;

        private bool update;

        protected override float Compute(FrooxEngineContext context)
        {
            if (!subscribed)
            {
                subscribed = true;
                Engine.Current.AudioSystem.AudioUpdate += () => 
                { 
                    update = true;
                };
            }

            IAudioSource audio = AudioInput.Evaluate(context);

            if (audio == null)
            {
                lastValue = 0f;
                return 0f;
            }

            if (!update) return lastValue;

            int amt = (int)(Engine.Current.AudioSystem.SampleRate * 0.01f);

            try
            {
                Span<MonoSample> buf = stackalloc MonoSample[amt];
                audio.Read(buf);
                float sum = 0;
                foreach (MonoSample sample in buf)
                {
                    sum += sample.AbsoluteAmplitude;
                }
                lastValue = sum / buf.Length;
            }
            catch (Exception e)
            {
                UniLog.Error(e.ToString());
                lastValue = -1f;
            }

            update = false;
            return lastValue;
        }
    }
}