using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class ClassOfIndex : ValueFunctionNode<ExecutionContext, ETrackedDeviceClass>
    {
        public ValueInput<uint> Index;

        protected override ETrackedDeviceClass Compute(ExecutionContext context)
        {
            return OpenVR.System.GetTrackedDeviceClass(Index.Evaluate(context));
        }
    }
}
