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
        public readonly ObjectInput<IWorldAudioDataSource> AudioInput;

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

            IWorldAudioDataSource audio = AudioInput.Evaluate(context);

            if (audio == null)
            {
                lastValue = 0f;
                return 0f;
            }

            if (!update) return lastValue;

            int amt = Engine.Current.AudioSystem.SimulationFrameSize;
            var simulator = Engine.Current.AudioSystem.Simulator;

            float sum = 0;

            try
            {
                switch (audio.ChannelCount)
                {
                    case 1:
                        Span<MonoSample> monoBuf = stackalloc MonoSample[amt];
                        audio.Read(monoBuf, simulator);
                        for (int i = 0; i < monoBuf.Length; i++)
                        {
                            sum += monoBuf[i].AbsoluteAmplitude;
                        }
                        break;
                    case 2:
                        Span<StereoSample> stereoBuf = stackalloc StereoSample[amt];
                        audio.Read(stereoBuf, simulator);
                        for (int i = 0; i < stereoBuf.Length; i++)
                        {
                            sum += stereoBuf[i].AbsoluteAmplitude;
                        }
                        break;
                    case 4:
                        Span<QuadSample> quadBuf = stackalloc QuadSample[amt];
                        audio.Read(quadBuf, simulator);
                        for (int i = 0; i < quadBuf.Length; i++)
                        {
                            sum += quadBuf[i].AbsoluteAmplitude;
                        }
                        break;
                    case 6:
                        Span<Surround51Sample> surroundBuf = stackalloc Surround51Sample[amt];
                        audio.Read(surroundBuf, simulator);
                        for (int i = 0; i < surroundBuf.Length; i++)
                        {
                            sum += surroundBuf[i].AbsoluteAmplitude;
                        }
                        break;
                }
                lastValue = sum / amt;
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