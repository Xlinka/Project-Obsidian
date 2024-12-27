using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class TestAudioProxy : ProtoFluxEngineProxy
    {
        private AudioOutput _output;
        private SineWaveClip _clip;
        private AudioClipPlayer _player;
        public readonly FieldDrive<float> FreqDrive;
        public readonly FieldDrive<float> AmpDrive;
        public bool Active
        {
            get { return _output.Enabled; }
            set { _output.Enabled = value; }
        }
        public bool IsValid => _clip.FilterWorldElement() != null && FreqDrive.Target != null && FreqDrive.IsLinkValid && AmpDrive.Target != null && AmpDrive.IsLinkValid;
        public float Frequency
        {
            get { return FreqDrive.Target.Value; }
            set { FreqDrive.Target.Value = value; }
        }
        public float Amplitude
        {
            get { return AmpDrive.Target.Value; }
            set { AmpDrive.Target.Value = value; }
        }
        protected override void OnAttach()
        {
            _output = Slot.AttachComponent<AudioOutput>();
            _clip = Slot.AttachComponent<SineWaveClip>();
            _player = Slot.AttachComponent<AudioClipPlayer>();
            _player.Play();
            _player.Loop = true;
            _player.Clip.Target = _clip;
            _output.Source.Target = _player;
            FreqDrive.Target = _clip.Frequency;
            FreqDrive.Target.Value = 440f;
            AmpDrive.Target = _clip.Amplitude;
            AmpDrive.Target.Value = 1f;
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class TestAudio : ProxyVoidNode<FrooxEngineContext, TestAudioProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        [@DefaultValue(440f)]
        public readonly ValueInput<float> Frequency;

        [ChangeListener]
        [@DefaultValue(1f)]
        public readonly ValueInput<float> Amplitude;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(TestAudioProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(TestAudioProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(TestAudioProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            TestAudioProxy proxy = GetProxy(context);
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
            TestAudioProxy proxy = GetProxy(context);
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
                proxy.Frequency = Frequency.Evaluate(context);
                proxy.Amplitude = Amplitude.Evaluate(context);
            }
            finally
            {
                context.World.UpdateManager.PopCurrentlyUpdating(proxy);
            }
        }
    }
}