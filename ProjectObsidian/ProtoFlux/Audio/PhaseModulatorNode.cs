using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using Obsidian.Elements;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class PhaseModulatorProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource AudioInput;

        public IAudioSource AudioInput2;

        public float ModulationIndex;

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => MathX.Min(AudioInput?.ChannelCount ?? 0, AudioInput2?.ChannelCount ?? 0);

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive || AudioInput == null || AudioInput2 == null)
            {
                buffer.Fill(default(S));
                return;
            }

            //buffer.Fill(default);

            Span<S> newBuffer = stackalloc S[buffer.Length];
            Span<S> newBuffer2 = stackalloc S[buffer.Length];
            newBuffer.Fill(default);
            newBuffer2.Fill(default);
            AudioInput.Read(newBuffer);
            AudioInput2.Read(newBuffer2);

            Algorithms.PhaseModulation(buffer, newBuffer, newBuffer2, ModulationIndex, ChannelCount);
        }
    }
    [NodeCategory("Obsidian/Audio/Effects")]
    public class PhaseModulator : ProxyVoidNode<FrooxEngineContext, PhaseModulatorProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput;

        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput2;

        [ChangeListener]
        public readonly ValueInput<float> ModulationIndex;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(PhaseModulatorProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(PhaseModulatorProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(PhaseModulatorProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            PhaseModulatorProxy proxy = GetProxy(context);
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
            PhaseModulatorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.AudioInput = AudioInput.Evaluate(context);
            proxy.AudioInput2 = AudioInput2.Evaluate(context);
            proxy.ModulationIndex = ModulationIndex.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            PhaseModulatorProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public PhaseModulator()
        {
            AudioOutput = new ObjectOutput<IAudioSource>(this);
        }
    }
}