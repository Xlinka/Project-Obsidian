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

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class FIR_FilterProxy : ProtoFluxEngineProxy, IAudioSource
    {
        public IAudioSource AudioInput;

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
                ((IFirFilter)filter).SetCoefficients(Coefficients.ToArray());
            }
        }

        public void OnElementsAddedOrRemoved(SyncElementList<Sync<float>> list, int startIndex, int count)
        {
            filters.Clear();
        }

        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive || AudioInput == null || !AudioInput.IsActive || Coefficients == null || Coefficients.Count == 0)
            {
                buffer.Fill(default(S));
                filters.Clear();
                return;
            }

            AudioInput.Read(buffer);

            if (!filters.TryGetValue(typeof(S), out var filter))
            {
                filter = new FirFilter<S>(Coefficients.ToArray());
                filters.Add(typeof(S), filter);
                UniLog.Log("Created new FIR filter");
            }

            if (!updateBools.TryGetValue(typeof(S), out bool update))
            {
                update = true;
                updateBools[typeof(S)] = update;
            }

            ((FirFilter<S>)filter).ProcessBuffer(buffer, update);

            if (update)
            {
                updateBools[typeof(S)] = false;
            }
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                foreach (var key in updateBools.Keys.ToArray())
                {
                    updateBools[key] = true;
                }
            };
        }
    }
    [NodeCategory("Obsidian/Audio/Filters")]
    public class FIR_Filter : ProxyVoidNode<FrooxEngineContext, FIR_FilterProxy>, IExecutionChangeListener<FrooxEngineContext>
    {
        [ChangeListener]
        public readonly ObjectInput<IAudioSource> AudioInput;

        public readonly ValueInput<int> CoefficientIndex;

        public readonly ValueInput<float> CoefficientValue;

        [PossibleContinuations(new string[] { "OnSetCoefficient" })]
        public readonly Operation SetCoefficient;

        [PossibleContinuations(new string[] { "OnClearCoefficients" })]
        public readonly Operation ClearCoefficients;

        public Continuation OnSetCoefficient;

        public Continuation OnClearCoefficients;

        public readonly ObjectOutput<IAudioSource> AudioOutput;

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
            float value = CoefficientValue.Evaluate(context);
            int prevCount = proxy.Coefficients.Count;
            proxy.Changed -= proxy.OnChanged;
            proxy.Coefficients.ElementsAdded -= proxy.OnElementsAddedOrRemoved;
            proxy.Coefficients.EnsureMinimumCount(index + 1);
            proxy.Coefficients.ElementsAdded += proxy.OnElementsAddedOrRemoved;
            proxy.Coefficients[index] = value;
            proxy.Changed += proxy.OnChanged;
            if (prevCount != proxy.Coefficients.Count)
            {
                proxy.filters.Clear();
            }
            else
            {
                foreach (var filter in proxy.filters.Values)
                {
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
            proxy.Changed -= proxy.OnChanged;
            proxy.Coefficients.ElementsRemoved -= proxy.OnElementsAddedOrRemoved;
            proxy.Coefficients.Clear();
            proxy.Coefficients.ElementsRemoved += proxy.OnElementsAddedOrRemoved;
            proxy.Changed += proxy.OnChanged;
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
            AudioOutput = new ObjectOutput<IAudioSource>(this);
            SetCoefficient = new Operation(this, 0);
            ClearCoefficients = new Operation(this, 1);
        }
    }
}