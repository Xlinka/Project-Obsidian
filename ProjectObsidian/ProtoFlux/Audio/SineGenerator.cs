using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class SineGeneratorProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public float Frequency;

        public float Amplitude;

        public float Phase;

        public float time;

        private float[] tempBuffer;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => 1;

        // TODO: Make this not advance time on each read
        // If two things are reading this generator, it advances twice as fast
        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            tempBuffer = tempBuffer.EnsureSize(buffer.Length);
            time %= MathX.PI * 2f;
            float advance = 1f / (float)base.Engine.AudioSystem.SampleRate * (MathX.PI * 2f) * (float)Frequency;
            for (int i = 0; i < buffer.Length; i++)
            {
                tempBuffer[i] = MathX.Sin(time + Phase) * MathX.Clamp01(Amplitude);
                time += advance;
            }
            double position = 0.0;
            MonoSample lastSample = default(MonoSample);
            MemoryMarshal.Cast<float, MonoSample>(MemoryExtensions.AsSpan(tempBuffer)).CopySamples(buffer, ref position, ref lastSample);
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class SineGenerator : ProxyVoidNode<FrooxEngineContext, SineGeneratorProxy>, IExecutionChangeListener<FrooxEngineContext>
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

        [PossibleContinuations(new string[] { "OnReset" })]
        public readonly Operation Reset;

        public Continuation OnReset;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(SineGeneratorProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(SineGeneratorProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(SineGeneratorProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            SineGeneratorProxy proxy = GetProxy(context);
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
            SineGeneratorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.Frequency = Frequency.Evaluate(context, 440f);
            proxy.Amplitude = Amplitude.Evaluate(context, 1f);
            proxy.Phase = Phase.Evaluate(context, 0f);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            SineGeneratorProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        private IOperation DoReset(FrooxEngineContext context)
        {
            SineGeneratorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return null;
            }
            proxy.time = 0f;
            return OnReset.Target;
        }

        public SineGenerator()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
            Reset = new Operation(this, 0);
        }
    }
}