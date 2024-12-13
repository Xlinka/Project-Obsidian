//using System;
//using Elements.Assets;
//using Elements.Core;
//using FrooxEngine;
//using FrooxEngine.ProtoFlux;
//using Obsidian.Components.Audio;
//using ProtoFlux.Core;
//using ProtoFlux.Runtimes.Execution;

//namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Playback
//{
//    [NodeCategory("Obsidian/Audio")]
//    public class TestAudio : ProxyVoidNode<FrooxEngineContext, TestAudio.Proxy>
//    {
//        public class Proxy : ProtoFluxEngineProxy, IAudioSource
//        {
//            public readonly SyncRef<IAudioSource> Source;
//            public readonly Sync<float> SmoothingFactor;

//            public bool IsActive => Source.Target != null && Source.Target.IsActive;

//            public int ChannelCount => Source.Target?.ChannelCount ?? 0;

//            public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
//            {
//                if (!IsActive)
//                {
//                    return;
//                }

//                Span<S> span = stackalloc S[buffer.Length];

//                span = buffer;

//                Source.Target.Read(span);

//                EMA_IIR_SmoothSignal.EMAIIRSmoothSignal(ref span, span.Length, SmoothingFactor);
//            }
//        }

//        public readonly ObjectInput<IAudioSource> Source;
//        public readonly ValueInput<float> SmoothingFactor;
//        public readonly ObjectOutput<IAudioSource> Result;

//        public bool SourceListensToChanges { get; private set; }

//        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

//        private ObjectStore<SlotEvent> _activeChangedHandler;

//        private bool ShouldListen(Proxy proxy)
//        {
//            if (proxy.Enabled && proxy.Slot.IsActive)
//            {
//                return proxy.Source.Target != null;
//            }
//            return false;
//        }

//        protected void UpdateListenerState(FrooxEngineContext context)
//        {
//            Proxy proxy = GetProxy(context);
//            if (proxy != null)
//            {
//                bool flag = ShouldListen(proxy);
//                if (flag != SourceListensToChanges)
//                {
//                    SourceListensToChanges = flag;
//                    context.Group.MarkChangeTrackingDirty();
//                }
//            }
//        }

//        protected override void ProxyAdded(Proxy proxy, FrooxEngineContext context)
//        {
//            NodeContextPath path = context.CaptureContextPath();
//            ProtoFluxNodeGroup group = context.Group;
//            context.GetEventDispatcher(out var dispatcher);
//            Action<IChangeable> value2 = delegate
//            {
//                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
//                {
//                    UpdateListenerState(c);
//                });
//            };
//            SlotEvent value3 = delegate
//            {
//                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
//                {
//                    UpdateListenerState(c);
//                });
//            };
//            proxy.EnabledField.Changed += value2;
//            proxy.Slot.ActiveChanged += value3;
//            _enabledChangedHandler.Write(value2, context);
//            _activeChangedHandler.Write(value3, context);
//            SourceListensToChanges = ShouldListen(proxy);
//        }

//        protected override void ProxyRemoved(Proxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
//        {
//            if (!inUseByAnotherInstance)
//            {
//                proxy.EnabledField.Changed -= _enabledChangedHandler.Read(context);
//                proxy.Slot.ActiveChanged -= _activeChangedHandler.Read(context);
//                _enabledChangedHandler.Clear(context);
//                _activeChangedHandler.Clear(context);
//            }
//        }

//        public void Changed(FrooxEngineContext context)
//        {
//            Proxy proxy = GetProxy(context);
//            if (proxy == null || proxy.IsRemoved)
//            {
//                return;
//            }
//            IAudioSource source = proxy.Source.Target;
//            if (source == null || !proxy.Source.Target.IsActive)
//            {
//                return;
//            }
//            try
//            {
//                context.World.UpdateManager.NestCurrentlyUpdating(proxy);
//            }
//            finally
//            {
//                context.World.UpdateManager.PopCurrentlyUpdating(proxy);
//            }
//        }

//        protected override void ComputeOutputs(FrooxEngineContext context)
//        {
//            Proxy proxy = GetProxy(context);
//        }
//    }
//}
