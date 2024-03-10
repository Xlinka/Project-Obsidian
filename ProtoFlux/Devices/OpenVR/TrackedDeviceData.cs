using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution;
using ProtoFlux.Core;

namespace OpenvrDataGetter
{
    [NodeCategory("OpenvrDataGetter")]
    public abstract class TrackedDeviceData<T> : ObjectFunctionNode<FrooxEngineContext, T>
    {
        public readonly ValueInput<uint> Index;

       
    }
}
