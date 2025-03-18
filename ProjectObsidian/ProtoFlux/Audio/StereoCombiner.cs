using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class StereoCombinerProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource Left;

        public IAudioSource Right;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => 2;

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<StereoSample> samples = stackalloc StereoSample[buffer.Length];
            Span<MonoSample> newBuffer = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer2 = stackalloc MonoSample[buffer.Length];
            samples.Fill(default);
            newBuffer.Fill(default);
            newBuffer2.Fill(default);
            if (Left != null && Left.ChannelCount == 1)
            {
                Left.Read(newBuffer);
            }
            if (Right != null && Right.ChannelCount == 1)
            {
                Right.Read(newBuffer2);
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                samples[i] = samples[i].SetChannel(0, newBuffer[i][0]);
                samples[i] = samples[i].SetChannel(1, newBuffer2[i][0]);
            }

            double position = 0.0;
            StereoSample lastSample = default(StereoSample);
            samples.CopySamples(buffer, ref position, ref lastSample);
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class StereoCombiner : ProxyVoidNode<FrooxEngineContext, StereoCombinerProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> Left;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> Right;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(StereoCombinerProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(StereoCombinerProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(StereoCombinerProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            StereoCombinerProxy proxy = GetProxy(context);
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
            StereoCombinerProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.Left = Left.Evaluate(context);
            proxy.Right = Right.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            StereoCombinerProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public StereoCombiner()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}