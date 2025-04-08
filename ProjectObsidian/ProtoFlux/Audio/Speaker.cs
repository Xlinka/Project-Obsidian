using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class SpeakerProxy : ProtoFluxEngineProxy
    {
        public readonly FieldDrive<bool> ActiveDrive;
        public readonly FieldDrive<float> VolDrive;
        public readonly DriveRef<SyncRef<IWorldAudioDataSource>> SourceDrive;
        public bool Active
        {
            get { return ActiveDrive.Target?.Value ?? false; }
            set { if (ActiveDrive.Target != null && ActiveDrive.IsLinkValid) ActiveDrive.Target.Value = value; }
        }
        public float Volume
        {
            get { return VolDrive.Target?.Value ?? 0f; }
            set { if (VolDrive.Target != null && VolDrive.IsLinkValid) VolDrive.Target.Value = value; }
        }
        public IWorldAudioDataSource Source
        {
            get { return SourceDrive.Target?.Target; }
            set { if (SourceDrive.Target != null && SourceDrive.IsLinkValid) SourceDrive.Target.Target = value; }
        }
        protected override void OnAttach()
        {
            var output = Slot.AttachComponent<AudioOutput>();
            ActiveDrive.Target = output.EnabledField;
            VolDrive.Target = output.Volume;
            SourceDrive.Target = output.Source;
            VolDrive.Target.Value = 1f;
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class Speaker : ProxyVoidNode<FrooxEngineContext, SpeakerProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> Source;

        [ChangeListener]
        [DefaultValueAttribute(1f)]
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
            try
            {
                context.World.UpdateManager.NestCurrentlyUpdating(proxy);
                proxy.Active = ValueListensToChanges;
            }
            finally
            {
                context.World.UpdateManager.PopCurrentlyUpdating(proxy);
            }
        }

        protected override void ProxyRemoved(SpeakerProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
        {
            if (!inUseByAnotherInstance)
            {
                proxy.EnabledField.Changed -= _enabledChangedHandler.Read(context);
                proxy.Slot.ActiveChanged -= _activeChangedHandler.Read(context);
                _enabledChangedHandler.Clear(context);
                _activeChangedHandler.Clear(context);
                try
                {
                    context.World.UpdateManager.NestCurrentlyUpdating(proxy);
                    proxy.Active = false;
                }
                finally
                {
                    context.World.UpdateManager.PopCurrentlyUpdating(proxy);
                }
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
                    try
                    {
                        context.World.UpdateManager.NestCurrentlyUpdating(proxy);
                        proxy.Active = shouldListen;
                    }
                    finally
                    {
                        context.World.UpdateManager.PopCurrentlyUpdating(proxy);
                    }
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
            try
            {
                context.World.UpdateManager.NestCurrentlyUpdating(proxy);
                proxy.Source = Source.Evaluate(context);
                proxy.Volume = Volume.Evaluate(context, 1f);
            }
            finally
            {
                context.World.UpdateManager.PopCurrentlyUpdating(proxy);
            }
        }
    }
}