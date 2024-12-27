using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class SpeakerProxy : ProtoFluxEngineProxy
    {
        private AudioOutput _output;
        public bool Active
        {
            get { return Output?.Enabled ?? false; }
            set { if (Output != null) Output.Enabled = value; }
        }
        public float Volume
        {
            get { return Output?.Volume.Value ?? 0f; }
            set { if (Output != null) Output.Volume.Value = value; }
        }
        public IAudioSource Source
        {
            get { return Output?.Source.Target; }
            set { if (Output != null) Output.Source.Target = value; }
        }
        public AudioOutput Output
        {
            get 
            {
                if (_output == null)
                {
                    _output = Slot.GetComponent<AudioOutput>();
                }
                return _output;
            }
        }
        protected override void OnAttach()
        {
            _output = Slot.AttachComponent<AudioOutput>();
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class Speaker : ProxyVoidNode<FrooxEngineContext, SpeakerProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> Source;

        [ChangeListener]
        public readonly ValueInput<float> Volume;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(SpeakerProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(SpeakerProxy proxy, FrooxEngineContext context)
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
        }

        protected override void ProxyRemoved(SpeakerProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
        {
            if (!inUseByAnotherInstance)
            {
                proxy.EnabledField.Changed -= _enabledChangedHandler.Read(context);
                proxy.Slot.ActiveChanged -= _activeChangedHandler.Read(context);
                _enabledChangedHandler.Clear(context);
                _activeChangedHandler.Clear(context);
            }
        }

        protected void UpdateListenerState(FrooxEngineContext context)
        {
            SpeakerProxy proxy = GetProxy(context);
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
            SpeakerProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            if (!proxy.IsValid)
            {
                return;
            }
            try
            {
                context.World.UpdateManager.NestCurrentlyUpdating(proxy);
                proxy.Source = Source.Evaluate(context);
                proxy.Volume = Volume.Evaluate(context);
            }
            finally
            {
                context.World.UpdateManager.PopCurrentlyUpdating(proxy);
            }
        }
    }
}