using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class Surround51_CombinerProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource LeftFront;

        public IAudioSource RightFront;

        public IAudioSource Center;

        public IAudioSource Subwoofer;

        public IAudioSource LeftRear;

        public IAudioSource RightRear;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => 6;

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<Surround51Sample> samples = stackalloc Surround51Sample[buffer.Length];
            Span<MonoSample> newBuffer = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer2 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer3 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer4 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer5 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer6 = stackalloc MonoSample[buffer.Length];
            samples.Fill(default);
            newBuffer.Fill(default);
            newBuffer2.Fill(default);
            newBuffer3.Fill(default);
            newBuffer4.Fill(default);
            newBuffer5.Fill(default);
            newBuffer6.Fill(default);
            if (LeftFront != null && LeftFront.ChannelCount == 1)
            {
                LeftFront.Read(newBuffer);
            }
            else
            {
                newBuffer.Fill(default);
            }
            if (RightFront != null && RightFront.ChannelCount == 1)
            {
                RightFront.Read(newBuffer2);
            }
            else
            {
                newBuffer2.Fill(default);
            }
            if (Center != null && Center.ChannelCount == 1)
            {
                Center.Read(newBuffer3);
            }
            else
            {
                newBuffer3.Fill(default);
            }
            if (Subwoofer != null && Subwoofer.ChannelCount == 1)
            {
                Subwoofer.Read(newBuffer4);
            }
            else
            {
                newBuffer4.Fill(default);
            }
            if (LeftRear != null && LeftRear.ChannelCount == 1)
            {
                LeftRear.Read(newBuffer5);
            }
            else
            {
                newBuffer5.Fill(default);
            }
            if (RightRear != null && RightRear.ChannelCount == 1)
            {
                RightRear.Read(newBuffer6);
            }
            else
            {
                newBuffer6.Fill(default);
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                //samples[i] = new Surround51Sample(leftFrontBuf[i][0], rightFrontBuf[i][0], centerBuf[i][0], subwooferBuf[i][0], leftRearBuf[i][0], rightRearBuf[i][0]);
                samples[i] = samples[i].SetChannel(0, newBuffer[i][0]);
                samples[i] = samples[i].SetChannel(1, newBuffer2[i][0]);
                samples[i] = samples[i].SetChannel(2, newBuffer3[i][0]);
                samples[i] = samples[i].SetChannel(3, newBuffer4[i][0]);
                samples[i] = samples[i].SetChannel(4, newBuffer5[i][0]);
                samples[i] = samples[i].SetChannel(5, newBuffer6[i][0]);
            }

            double position = 0.0;
            Surround51Sample lastSample = default(Surround51Sample);
            samples.CopySamples(buffer, ref position, ref lastSample);
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class Surround51_Combiner : ProxyVoidNode<FrooxEngineContext, Surround51_CombinerProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> LeftFront;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> RightFront;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> Center;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> Subwoofer;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> LeftRear;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> RightRear;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(Surround51_CombinerProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(Surround51_CombinerProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(Surround51_CombinerProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            Surround51_CombinerProxy proxy = GetProxy(context);
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
            Surround51_CombinerProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.LeftFront = LeftFront.Evaluate(context);
            proxy.RightFront = RightFront.Evaluate(context);
            proxy.Center = Center.Evaluate(context);
            proxy.Subwoofer = Subwoofer.Evaluate(context);
            proxy.LeftRear = LeftRear.Evaluate(context);
            proxy.RightRear = RightRear.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            Surround51_CombinerProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public Surround51_Combiner()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}