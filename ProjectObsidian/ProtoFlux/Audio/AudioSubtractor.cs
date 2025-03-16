﻿using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class AudioSubtractorProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource AudioInput;

        public IAudioSource AudioInput2;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => AudioInput?.ChannelCount ?? AudioInput2?.ChannelCount ?? 0;

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            if (AudioInput == null && AudioInput2 == null)
            {
                buffer.Fill(default(S));
                return;
            }

            buffer.Fill(default);

            Span<S> buffer1s = stackalloc S[buffer.Length];
            buffer1s.Fill(default);
            if (AudioInput != null)
            {
                AudioInput.Read(buffer1s);
            }

            Span<S> buffer2s = stackalloc S[buffer.Length];
            buffer2s.Fill(default);
            if (AudioInput2 != null)
            {
                AudioInput2.Read(buffer2s);
            }
            
            for (int i = 0; i < buffer.Length; i+=buffer[i].ChannelCount)
            {
                buffer[i] = buffer1s[i].Subtract(buffer2s[i]);
                
                //for (int j = 0; j < ChannelCount; j++)
                //{
                //    if (newBuffer[i][j] > 1f) newBuffer[i] = newBuffer[i].SetChannel(j, 1f);
                //    else if (newBuffer[i][j] < -1f) newBuffer[i] = newBuffer[i].SetChannel(j, -1f);
                //}
            }
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class AudioSubtractor : ProxyVoidNode<FrooxEngineContext, AudioSubtractorProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput2;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(AudioSubtractorProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(AudioSubtractorProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(AudioSubtractorProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            AudioSubtractorProxy proxy = GetProxy(context);
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
            AudioSubtractorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.AudioInput = AudioInput.Evaluate(context);
            proxy.AudioInput2 = AudioInput2.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            AudioSubtractorProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public AudioSubtractor()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}