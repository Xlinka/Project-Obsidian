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
    public class FIR_FilterProxy : ProtoFluxEngineProxy, Awwdio.IAudioDataSource, IWorldAudioDataSource
    {
        public IWorldAudioDataSource AudioInput;

        public readonly SyncFieldList<float> Coefficients;

        public Dictionary<Type, object> filters = new();

        public Dictionary<Type, bool> updateBools = new();

        public bool Active;

        public bool IsActive => Active;

        public int ChannelCount => AudioInput?.ChannelCount ?? 0;

        protected override void OnAwake()
        {
            Coefficients.Changed += OnChanged;
            Coefficients.ElementsAdded += OnElementsAddedOrRemoved;
            Coefficients.ElementsRemoved += OnElementsAddedOrRemoved;
            base.OnAwake();
        }

        public void OnChanged(IChangeable changeable)
        {
            foreach (var filter in filters.Values)
            {
                lock ((IFirFilter)filter)
                    ((IFirFilter)filter).SetCoefficients(Coefficients.ToArray());
            }
        }

        public void OnElementsAddedOrRemoved(SyncElementList<Sync<float>> list, int startIndex, int count)
        {
            lock (filters)
                filters.Clear();
        }

        public void Read<S>(Span<S> buffer, AudioSimulator simulator) where S : unmanaged, IAudioSample<S>
        {
            float[] coeffs = null;
            lock (Coefficients)
            {
                if (Coefficients != null && !Coefficients.IsDisposed)
                    coeffs = Coefficients.ToArray();
            }
            if (!IsActive || AudioInput == null || !AudioInput.IsActive || coeffs == null || coeffs.Length == 0)
            {
                buffer.Fill(default(S));
                lock (filters)
                    filters.Clear();
                return;
            }

            AudioInput.Read(buffer, simulator);

            object filter;
            lock (filters)
            {
                if (!filters.TryGetValue(typeof(S), out filter))
                {
                    filter = new FirFilter<S>(coeffs);
                    filters.Add(typeof(S), filter);
                    UniLog.Log("Created new FIR filter");
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

            lock ((FirFilter<S>)filter)
            {
                ((FirFilter<S>)filter).ProcessBuffer(buffer, update);
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
    [NodeCategory("Obsidian/Audio/Filters")]
    public class FIR_Filter : ProxyVoidNode<FrooxEngineContext, FIR_FilterProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> AudioInput;

        public readonly ValueInput<int> CoefficientIndex;

        public readonly ValueInput<float> CoefficientValue;

        [PossibleContinuations(new string[] { "OnSetCoefficient" })]
        public readonly Operation SetCoefficient;

        [PossibleContinuations(new string[] { "OnClearCoefficients" })]
        public readonly Operation ClearCoefficients;

        public Continuation OnSetCoefficient;

        public Continuation OnClearCoefficients;

        public readonly ObjectOutput<IWorldAudioDataSource> AudioOutput;

        private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

        private ObjectStore<SlotEvent> _activeChangedHandler;

        public bool ValueListensToChanges { get; private set; }

        private bool ShouldListen(FIR_FilterProxy proxy)
        {
            if (proxy.Enabled)
            {
                return proxy.Slot.IsActive;
            }
            return false;
        }

        protected override void ProxyAdded(FIR_FilterProxy proxy, FrooxEngineContext context)
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

        protected override void ProxyRemoved(FIR_FilterProxy proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
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
            FIR_FilterProxy proxy = GetProxy(context);
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

        private IOperation DoSetCoefficient(FrooxEngineContext context)
        {
            FIR_FilterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return null;
            }
            var index = CoefficientIndex.Evaluate(context);
            if (index < 0) return null;
            float value = CoefficientValue.Evaluate(context);
            int prevCount = proxy.Coefficients.Count;
            proxy.Coefficients.Changed -= proxy.OnChanged;
            proxy.Coefficients.ElementsAdded -= proxy.OnElementsAddedOrRemoved;
            lock (proxy.Coefficients)
                proxy.Coefficients.EnsureMinimumCount(index + 1);
            proxy.Coefficients.ElementsAdded += proxy.OnElementsAddedOrRemoved;
            lock (proxy.Coefficients)
                proxy.Coefficients[index] = value;
            proxy.Coefficients.Changed += proxy.OnChanged;
            if (prevCount != proxy.Coefficients.Count)
            {
                lock (proxy.filters)
                    proxy.filters.Clear();
            }
            else
            {
                foreach (var filter in proxy.filters.Values)
                {
                    lock ((IFirFilter)filter)
                        ((IFirFilter)filter).SetCoefficients(proxy.Coefficients.ToArray());
                }
            }
            return OnSetCoefficient.Target;
        }

        private IOperation DoClearCoefficients(FrooxEngineContext context)
        {
            FIR_FilterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return null;
            }
            proxy.Coefficients.Changed -= proxy.OnChanged;
            proxy.Coefficients.ElementsRemoved -= proxy.OnElementsAddedOrRemoved;
            lock (proxy.Coefficients)
                proxy.Coefficients.Clear();
            proxy.Coefficients.ElementsRemoved += proxy.OnElementsAddedOrRemoved;
            proxy.Coefficients.Changed += proxy.OnChanged;
            lock (proxy.filters)
                proxy.filters.Clear();
            return OnClearCoefficients.Target;
        }

        public void Changed(FrooxEngineContext context)
        {
            FIR_FilterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            proxy.AudioInput = AudioInput.Evaluate(context);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            FIR_FilterProxy proxy = GetProxy(context);
            AudioOutput.Write(proxy, context);
        }

        public FIR_Filter()
        {
            AudioOutput = new ObjectOutput<IWorldAudioDataSource>(this);
            SetCoefficient = new Operation(this, 0);
            ClearCoefficients = new Operation(this, 1);
        }
    }
}