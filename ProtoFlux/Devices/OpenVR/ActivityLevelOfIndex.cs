using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class ActivityLevelOfIndex : ValueFunctionNode<ExecutionContext, EDeviceActivityLevel>
    {
        public ValueInput<uint> Index;

        protected override EDeviceActivityLevel Compute(ExecutionContext context)
        {
            return OpenVR.System.GetTrackedDeviceActivityLevel(Index.Evaluate(context));
        }
    }
}
