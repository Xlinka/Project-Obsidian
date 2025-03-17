﻿using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using System.Collections.Generic;
using SharpPipe;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class ReverbProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource AudioInput;

        public Dictionary<Type, object> reverbs = new();

        public ZitaParameters parameters;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => AudioInput?.ChannelCount ?? 0;

        private ZitaParameters defaultParameters = new ZitaParameters();

        public Dictionary<Type, object> lastBuffers = new();

        private bool update;

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive || AudioInput == null || !AudioInput.IsActive || parameters.Equals(defaultParameters))
            {
                buffer.Fill(default(S));
                reverbs.Clear();
                return;
            }

            //buffer.Fill(default);

            AudioInput.Read(buffer);

            if (!reverbs.TryGetValue(typeof(S), out var reverb))
            {
                reverb = new BufferReverber<S>(Engine.Current.AudioSystem.SampleRate, parameters);
                reverbs.Add(typeof(S), reverb);
                UniLog.Log("Created new reverb");
            }

            bool lastBufferIsNull = false;
            if (!lastBuffers.TryGetValue(typeof(S), out var lastBuffer))
            {
                lastBufferIsNull = true;
            }

            if (!update && !lastBufferIsNull)
            {
                ((S[])lastBuffer).CopyTo(buffer);
                return;
            }

            ((BufferReverber<S>)reverb).ApplyReverb(ref buffer);

            if (update || lastBufferIsNull)
            {
                update = false;
                //((S[])lastBuffer).EnsureExactSize(buffer.Length);
                lastBuffer = buffer.ToArray();
                lastBuffers[typeof(S)] = lastBuffer;
                //foreach (var type in reverbs.Keys)
                //{
                //    if (type == typeof(S)) continue;
                //    if (type == typeof(MonoSample))
                //    {
                //        ((S[])lastBuffer).AsSpan().CopySamples(((MonoSample[])reverbs[type]).AsSpan());
                //    }
                //    else if (type == typeof(StereoSample))
                //    {
                //        ((S[])lastBuffer).AsSpan().CopySamples(((StereoSample[])reverbs[type]).AsSpan());
                //    }
                //    else if (type == typeof(QuadSample))
                //    {
                //        ((S[])lastBuffer).AsSpan().CopySamples(((QuadSample[])reverbs[type]).AsSpan());
                //    }
                //    else if (type == typeof(Surround51Sample))
                //    {
                //        ((S[])lastBuffer).AsSpan().CopySamples(((Surround51Sample[])reverbs[type]).AsSpan());
                //    }
                //}
            }
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                update = true;
            };
        }
    }
    [NodeCategory("Obsidian/Audio/Effects")]
    public class Reverb : ProxyVoidNode<FrooxEngineContext, ReverbProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput;

        [ChangeListener]
        public readonly ValueInput<ZitaParameters> Parameters;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        private ZitaParameters lastParameters;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(ReverbProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(ReverbProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(ReverbProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            ReverbProxy proxy = GetProxy(context);
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
            ReverbProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.AudioInput = AudioInput.Evaluate(context);
            proxy.parameters = Parameters.Evaluate(context);
            if (!proxy.parameters.Equals(lastParameters))
            {
                proxy.reverbs.Clear();
            }
            lastParameters = proxy.parameters;
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            ReverbProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public Reverb()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}