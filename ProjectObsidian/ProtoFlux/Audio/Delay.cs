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
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class DelayProxy : ProtoFluxEngineProxy, Awwdio.IAudioDataSource, IWorldAudioDataSource
    {
        public IWorldAudioDataSource AudioInput;

        public int delayMilliseconds;

        public float feedback;

        public float DryWet;

        public Dictionary<Type, object> delays = new();

        public Dictionary<Type, bool> updateBools = new();

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => AudioInput?.ChannelCount ?? 0;

        public void Read<S>(Span<S> buffer, AudioSimulator simulator) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive || AudioInput == null || !AudioInput.IsActive)
            {
                buffer.Fill(default(S));
                lock (delays)
                    delays.Clear();
                return;
            }

            AudioInput.Read(buffer, simulator);

            object delay;
            lock (delays)
            {
                if (!delays.TryGetValue(typeof(S), out delay))
                {
                    delay = new DelayEffect<S>(delayMilliseconds, Engine.Current.AudioSystem.SampleRate);
                    delays.Add(typeof(S), delay);
                    UniLog.Log("Created new delay");
                }
            }

            bool update;
            lock (updateBools)
            {
                if (!updateBools.TryGetValue(typeof(S), out update))
                {
                    update = true;
                    updateBools[typeof(S)] = update;
                }
            }

            lock ((DelayEffect<S>)delay)
            {
                ((DelayEffect<S>)delay).Process(buffer, DryWet, feedback, update);
            }

            if (update)
            {
                lock (updateBools)
                    updateBools[typeof(S)] = false;
            }
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                foreach (var key in updateBools.Keys.ToArray())
                {
                    lock (updateBools)
                        updateBools[key] = true;
                }
            };
        }
    }
    [NodeCategory("Obsidian/Audio/Effects")]
    public class Delay : ProxyVoidNode<FrooxEngineContext, DelayProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> AudioInput;

        [ChangeListener]
        public readonly ValueInput<int> DelayMilliseconds;

        [ChangeListener]
        public readonly ValueInput<float> Feedback;

        [ChangeListener]
        public readonly ValueInput<float> DryWet;

        public readonly ObjectOutput<IWorldAudioDataSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(DelayProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(DelayProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(DelayProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            DelayProxy proxy = GetProxy(context);
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
            DelayProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.AudioInput = AudioInput.Evaluate(context);
            proxy.delayMilliseconds = DelayMilliseconds.Evaluate(context);
            foreach (var delay in proxy.delays.Values)
            {
                ((IDelayEffect)delay).SetDelayTime(proxy.delayMilliseconds, Engine.Current.AudioSystem.SampleRate);
            }
            proxy.feedback = Feedback.Evaluate(context);
            proxy.DryWet = DryWet.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            DelayProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public Delay()
        {
            AudioOutput = new ObjectOutput<IWorldAudioDataSource>(this);
        }
    }
}