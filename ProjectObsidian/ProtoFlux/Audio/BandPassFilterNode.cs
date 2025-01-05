using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class BandPassFilterProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource AudioInput;

        public float LowFrequency;

        public float HighFrequency;

        public float Resonance;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => AudioInput.ChannelCount;

        private BandPassFilterController _controller = new();

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<S> newBuffer = stackalloc S[buffer.Length];
            newBuffer = buffer;
            if (AudioInput != null)
            {
                AudioInput.Read(newBuffer);
            }
            else
            {
                newBuffer.Fill(default);
            }

            _controller.Process(newBuffer, LowFrequency, HighFrequency, Resonance);
        }
    }
    [NodeCategory("Obsidian/Audio/Filters")]
    public class BandPassFilter : ProxyVoidNode<FrooxEngineContext, BandPassFilterProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput;

        [ChangeListener]
        [DefaultValueAttribute(20f)]
        public readonly ValueInput<float> LowFrequency;

        [ChangeListener]
        [DefaultValueAttribute(20000f)]
        public readonly ValueInput<float> HighFrequency;

        [ChangeListener]
        [DefaultValueAttribute(1.41f)]
        public readonly ValueInput<float> Resonance;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(BandPassFilterProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(BandPassFilterProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(BandPassFilterProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            BandPassFilterProxy proxy = GetProxy(context);
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
            BandPassFilterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.AudioInput = AudioInput.Evaluate(context);
            proxy.LowFrequency = LowFrequency.Evaluate(context, 20f);
            proxy.HighFrequency = HighFrequency.Evaluate(context, 20000f);
            proxy.Resonance = Resonance.Evaluate(context, 1.41f);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            BandPassFilterProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public BandPassFilter()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}