using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using FrooxEngine;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using Elements.Assets;
using System;
using Elements.Data;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    [DataModelType]
    public enum LevelMonitorMode
    {
        Average,
        Peak,
        RMS
    }
    [ContinuouslyChanging]
    [NodeCategory("Obsidian/Audio")]
    public class LevelMonitor : ValueFunctionNode<FrooxEngineContext, float>
    {
        public readonly ObjectInput<IWorldAudioDataSource> AudioInput;

        public readonly ValueInput<LevelMonitorMode> Mode;

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
            var mode = Mode.Evaluate(context);

            if (audio == null)
            {
                lastValue = 0f;
                return 0f;
            }

            if (!update) return lastValue;

            int amt = Engine.Current.AudioSystem.SimulationFrameSize;
            var simulator = Engine.Current.AudioSystem.Simulator;

            float sumOfSquares = 0;
            float absSum = 0;
            float peak = 0;

            try
            {
                switch (audio.ChannelCount)
                {
                    case 1:
                        Span<MonoSample> monoBuf = stackalloc MonoSample[amt];
                        audio.Read(monoBuf, simulator);
                        for (int i = 0; i < monoBuf.Length; i++)
                        {
                            absSum += monoBuf[i].AbsoluteAmplitude;
                            sumOfSquares += MathX.Pow(monoBuf[i].AbsoluteAmplitude, 2);
                            if (monoBuf[i].AbsoluteAmplitude > peak)
                                peak = monoBuf[i].AbsoluteAmplitude;
                        }
                        break;
                    case 2:
                        Span<StereoSample> stereoBuf = stackalloc StereoSample[amt];
                        audio.Read(stereoBuf, simulator);
                        for (int i = 0; i < stereoBuf.Length; i++)
                        {
                            absSum += stereoBuf[i].AbsoluteAmplitude;
                            sumOfSquares += MathX.Pow(stereoBuf[i].AbsoluteAmplitude, 2);
                            if (stereoBuf[i].AbsoluteAmplitude > peak)
                                peak = stereoBuf[i].AbsoluteAmplitude;
                        }
                        break;
                    case 4:
                        Span<QuadSample> quadBuf = stackalloc QuadSample[amt];
                        audio.Read(quadBuf, simulator);
                        for (int i = 0; i < quadBuf.Length; i++)
                        {
                            absSum += quadBuf[i].AbsoluteAmplitude;
                            sumOfSquares += MathX.Pow(quadBuf[i].AbsoluteAmplitude, 2);
                            if (quadBuf[i].AbsoluteAmplitude > peak)
                                peak = quadBuf[i].AbsoluteAmplitude;
                        }
                        break;
                    case 6:
                        Span<Surround51Sample> surroundBuf = stackalloc Surround51Sample[amt];
                        audio.Read(surroundBuf, simulator);
                        for (int i = 0; i < surroundBuf.Length; i++)
                        {
                            absSum += surroundBuf[i].AbsoluteAmplitude;
                            sumOfSquares += MathX.Pow(surroundBuf[i].AbsoluteAmplitude, 2);
                            if (surroundBuf[i].AbsoluteAmplitude > peak)
                                peak = surroundBuf[i].AbsoluteAmplitude;
                        }
                        break;
                }
                switch (mode)
                {
                    case LevelMonitorMode.Average:
                        lastValue = absSum / amt;
                        break;
                    case LevelMonitorMode.RMS:
                        lastValue = MathX.Sqrt(sumOfSquares / amt);
                        break;
                    case LevelMonitorMode.Peak:
                        lastValue = peak;
                        break;
                    default:
                        lastValue = -1f;
                        break;
                }
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