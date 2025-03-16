using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using System.Runtime.InteropServices;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class SquareGeneratorProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public float Frequency;

        public float Amplitude;

        public float Phase;

        public float PulseWidth;

        public double time;

        private float[] tempBuffer = null;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => 1;

        private bool updateTime;

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            buffer.Fill(default);

            if (!updateTime && tempBuffer != null)
            {
                double position2 = 0.0;
                MonoSample lastSample2 = default(MonoSample);
                MemoryMarshal.Cast<float, MonoSample>(MemoryExtensions.AsSpan(tempBuffer)).CopySamples<MonoSample, S>(buffer, ref position2, ref lastSample2, 1.0);
                return;
            }

            tempBuffer = tempBuffer.EnsureSize(buffer.Length);
            var temptime = time;
            float period = (1f / Frequency);
            temptime %= period;
            var clampedAmplitude = MathX.Clamp01(Amplitude);
            float advance = (1f / (float)base.Engine.AudioSystem.SampleRate);
            
            for (int i = 0; i < buffer.Length; i++)
            {
                if ((temptime + (Phase * period)) % period <= PulseWidth / Frequency)
                {
                    tempBuffer[i] = 1f * clampedAmplitude;
                }
                else
                {
                    tempBuffer[i] = -1f * clampedAmplitude;
                }
                temptime += advance;
            }
            if (updateTime)
            {
                time = temptime;
                updateTime = false;
            }
            double position = 0.0;
            MonoSample lastSample = default(MonoSample);
            MemoryMarshal.Cast<float, MonoSample>(MemoryExtensions.AsSpan(tempBuffer)).CopySamples(buffer, ref position, ref lastSample);
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                updateTime = true;
            };
        }
    }
    [NodeCategory("Obsidian/Audio/Generators")]
    public class SquareGenerator : ProxyVoidNode<FrooxEngineContext, SquareGeneratorProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        [DefaultValueAttribute(440f)]
        public readonly ValueInput<float> Frequency;

        [ChangeListener]
        [DefaultValueAttribute(1f)]
        public readonly ValueInput<float> Amplitude;

        [ChangeListener]
        [DefaultValueAttribute(0f)]
        public readonly ValueInput<float> Phase;

        [ChangeListener]
        [DefaultValueAttribute(0.5f)]
        public readonly ValueInput<float> PulseWidth;

        [PossibleContinuations(new string[] { "OnReset" })]
        public readonly Operation Reset;

        public Continuation OnReset;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(SquareGeneratorProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(SquareGeneratorProxy proxy, FrooxEngineContext context)
        {
            base.ProxyAdded(proxy, context);
            NodeContextPath path = context.CaptureContextPath();
            ProtoFluxNodeGroup group = context.Group;
            context.GetEventDispatcher(out var dispatcher);
            Action<IChangeable> enabledHandler = delegate
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    UpdateListenerState(c);
                });
            };
            SlotEvent activeHandler = delegate
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    UpdateListenerState(c);
                });
            };
            proxy.EnabledField.Changed += enabledHandler;
            proxy.Slot.ActiveChanged += activeHandler;
            _enabledChangedHandler.Write(enabledHandler, context);
            _activeChangedHandler.Write(activeHandler, context);
            ValueListensToChanges = ShouldListen(proxy);
            proxy.Active = ValueListensToChanges;
        }

        protected override void ProxyRemoved(SquareGeneratorProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
        {
            if (!inUseByAnotherInstance)
            {
                proxy.EnabledField.Changed -= _enabledChangedHandler.Read(context);
                proxy.Slot.ActiveChanged -= _activeChangedHandler.Read(context);
                _enabledChangedHandler.Clear(context);
                _activeChangedHandler.Clear(context);
                proxy.Active = false;
            }
        }

        protected void UpdateListenerState(FrooxEngineContext context)
        {
            SquareGeneratorProxy proxy = GetProxy(context);
            if (proxy != null)
            {
                bool shouldListen = ShouldListen(proxy);
                if (shouldListen != ValueListensToChanges)
                {
                    ValueListensToChanges = shouldListen;
                    context.Group.MarkChangeTrackingDirty();
                    proxy.Active = shouldListen;
                }
            }
        }

        public void Changed(FrooxEngineContext context)
        {
            SquareGeneratorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.Amplitude = Amplitude.Evaluate(context, 1f);
            proxy.Phase = Phase.Evaluate(context, 0f);
            proxy.Frequency = Frequency.Evaluate(context, 440f);
            proxy.PulseWidth = PulseWidth.Evaluate(context, 0.5f);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            SquareGeneratorProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        private IOperation DoReset(FrooxEngineContext context)
        {
            SquareGeneratorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return null;
            }
            proxy.time = 0f;
            return OnReset.Target;
        }

        public SquareGenerator()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
            Reset = new Operation(this, 0);
        }
    }
}