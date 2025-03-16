using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;
using Elements.Core;
using System.Collections.Generic;
using System.Linq;
using SkyFrost.Base;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class AudioClampProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource AudioInput;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => AudioInput?.ChannelCount ?? 0;

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive || AudioInput == null || !AudioInput.IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            buffer.Fill(default);

            //Span<float> buffer1 = stackalloc float[buffer.Length * AudioInput.ChannelCount];
            //buffer1.Fill(default);
            //AudioInput.GetFloatBuffer(buffer1);
            //AudioInput.CopyFloatToBuffer(buffer1, buffer);

            AudioInput.Read(buffer);

            for (int i = 0; i < buffer.Length; i++)
            {
                for (int j = 0; j < ChannelCount; j++)
                {
                    if (buffer[i][j] > 1f) buffer[i] = buffer[i].SetChannel(j, 1f);
                    else if (buffer[i][j] < -1f) buffer[i] = buffer[i].SetChannel(j, -1f);
                }
            }
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class AudioClamp : ProxyVoidNode<FrooxEngineContext, AudioClampProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(AudioClampProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(AudioClampProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(AudioClampProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            AudioClampProxy proxy = GetProxy(context);
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
            AudioClampProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.AudioInput = AudioInput.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            AudioClampProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public AudioClamp()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}