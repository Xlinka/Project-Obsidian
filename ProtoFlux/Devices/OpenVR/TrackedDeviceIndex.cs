using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace OpenvrDataGetter.ProtoFlux
{
    [NodeName("TrackedDeviceIndex")]
    [Category("ProtoFlux/OpenVR")]
    public class TrackedDeviceIndex : ValueFunctionNode<ExecutionContext, uint>
    {
        public readonly ValueInput<uint> Index;

        protected override uint Compute(ExecutionContext context)
        {
            return Index.Evaluate(context); 
        }
    }
}
