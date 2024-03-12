using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace OpenvrDataGetter.Nodes
{
    [NodeCategory("OpenvrDataGetter")]
    public abstract class TrackedDeviceData<T> : ObjectFunctionNode<ExecutionContext, T>
    {
        public readonly ValueInput<uint> Index;
    }
}