using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class QuadCombinerProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource LeftFront;

        public IAudioSource RightFront;

        public IAudioSource LeftRear;

        public IAudioSource RightRear;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => 4;

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<QuadSample> samples = stackalloc QuadSample[buffer.Length];
            Span<S> leftFrontBuf = stackalloc S[buffer.Length];
            Span<S> rightFrontBuf = stackalloc S[buffer.Length];
            Span<S> leftRearBuf = stackalloc S[buffer.Length];
            Span<S> rightRearBuf = stackalloc S[buffer.Length];
            if (LeftFront != null)
            {
                LeftFront.Read(leftFrontBuf);
            }
            else
            {
                leftFrontBuf.Fill(default);
            }
            if (RightFront != null)
            {
                RightFront.Read(rightFrontBuf);
            }
            else
            {
                rightFrontBuf.Fill(default);
            }
            if (LeftRear != null)
            {
                LeftRear.Read(leftRearBuf);
            }
            else
            {
                leftRearBuf.Fill(default);
            }
            if (RightRear != null)
            {
                RightRear.Read(rightRearBuf);
            }
            else
            {
                rightRearBuf.Fill(default);
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                samples[i] = new QuadSample(leftFrontBuf[i][0], rightFrontBuf[i][0], leftRearBuf[i][0], rightRearBuf[i][0]);
            }

            double position = 0.0;
            QuadSample lastSample = default(QuadSample);
            samples.CopySamples(buffer, ref position, ref lastSample);
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class QuadCombiner : ProxyVoidNode<FrooxEngineContext, QuadCombinerProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> LeftFront;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> RightFront;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> LeftRear;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> RightRear;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(QuadCombinerProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(QuadCombinerProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(QuadCombinerProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            QuadCombinerProxy proxy = GetProxy(context);
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
            QuadCombinerProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.LeftFront = LeftFront.Evaluate(context);
            proxy.RightFront = RightFront.Evaluate(context);
            proxy.LeftRear = LeftRear.Evaluate(context);
            proxy.RightRear = RightRear.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            QuadCombinerProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public QuadCombiner()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}