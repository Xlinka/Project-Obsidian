using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using System.Collections.Generic;
using SharpPipe;
using System.Linq;
using Awwdio;
using Obsidian.Elements;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class ReverbProxy : ProtoFluxEngineProxy, Awwdio.IAudioDataSource, IWorldAudioDataSource
    {
        public IWorldAudioDataSource AudioInput;

        public ZitaParameters parameters;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => AudioInput?.ChannelCount ?? 0;

        private ZitaParameters defaultParameters = new ZitaParameters();

        public ReverbController _controller = new();

        public void Read<S>(Span<S> buffer, AudioSimulator simulator) where S : unmanaged, IAudioSample<S>
        {
            lock (_controller)
            {
                if (AudioInput == null)
                {
                    _controller.Clear();
                }
                if (!IsActive || AudioInput == null || !AudioInput.IsActive)
                {
                    buffer.Fill(default(S));
                    return;
                }

                if (parameters.Equals(defaultParameters)) // if nothing is plugged into parameters, use the default output of the ConstructZitaParameters node
                {
                    parameters = new ZitaParameters
                    {
                        InDelay = 0,
                        Crossover = 200,
                        RT60Low = 1.49f,
                        RT60Mid = 1.2f,
                        HighFrequencyDamping = 6000,
                        EQ1Frequency = 250,
                        EQ1Level = 0,
                        EQ2Frequency = 5000,
                        EQ2Level = 0,
                        Mix = 0.7f,
                        Level = 8
                    };
                }

                AudioInput.Read(buffer, simulator);

                _controller.Process(buffer, parameters);
            }
            
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                lock (_controller)
                {
                    foreach (var key in _controller.reverbTypes)
                    {
                        _controller.updateBools[key] = true;
                    }
                }
            };
        }
    }
    [NodeCategory("Obsidian/Audio/Effects")]
    public class Reverb : ProxyVoidNode<FrooxEngineContext, ReverbProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> AudioInput;

        [ChangeListener]
        public readonly ValueInput<ZitaParameters> Parameters;

        public readonly ObjectOutput<IWorldAudioDataSource> AudioOutput;

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
                lock (proxy._controller)
                    proxy._controller.Clear();
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
            AudioOutput = new ObjectOutput<IWorldAudioDataSource>(this);
        }
    }
}