using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace OpenvrDataGetter.ProtoFlux
{
    [NodeName("TrackedDeviceIndex")]
    [Category("ProtoFlux/OpenVR")]
    public class TrackedDeviceIndex : ValueFunctionNode<FrooxEngineContext, uint>
    {
        public readonly ValueInput<uint> Index;

        protected override uint Compute(FrooxEngineContext context)
        {
            return Index.Evaluate(context); 
        }
    }
}
