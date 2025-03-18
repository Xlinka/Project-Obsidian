using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class ChannelSplitterProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource AudioInput;

        public int Channel;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => 1;

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            if (AudioInput == null || AudioInput.ChannelCount < Channel + 1 || Channel < 0)
            {
                buffer.Fill(default(S));
                return;
            }

            switch (AudioInput.ChannelCount)
            {
                case 1:
                    Span<MonoSample> monoBuf = stackalloc MonoSample[buffer.Length];
                    AudioInput.Read(monoBuf);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = buffer[i].SetChannel(0, monoBuf[i][Channel]);
                    }
                    break;
                case 2:
                    Span<StereoSample> stereoBuf = stackalloc StereoSample[buffer.Length];
                    AudioInput.Read(stereoBuf);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = buffer[i].SetChannel(0, stereoBuf[i][Channel]);
                    }
                    break;
                case 4:
                    Span<QuadSample> quadBuf = stackalloc QuadSample[buffer.Length];
                    AudioInput.Read(quadBuf);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = buffer[i].SetChannel(0, quadBuf[i][Channel]);
                    }
                    break;
                case 6:
                    Span<Surround51Sample> surroundBuf = stackalloc Surround51Sample[buffer.Length];
                    AudioInput.Read(surroundBuf);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = buffer[i].SetChannel(0, surroundBuf[i][Channel]);
                    }
                    break;
            }
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class ChannelSplitter : ProxyVoidNode<FrooxEngineContext, ChannelSplitterProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput;

        [ChangeListener]
        public readonly ValueInput<int> Channel;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(ChannelSplitterProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(ChannelSplitterProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(ChannelSplitterProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            ChannelSplitterProxy proxy = GetProxy(context);
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
            ChannelSplitterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.AudioInput = AudioInput.Evaluate(context);
            proxy.Channel = Channel.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            ChannelSplitterProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public ChannelSplitter()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}