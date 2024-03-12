using OpenvrDataGetter.Nodes;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class IsIndexConnected : TrackedDeviceData<bool>
    {
        protected override bool Compute(ExecutionContext context)
        {
            uint deviceIndex = Index.Evaluate(context);
            bool isConnected = OpenVR.System.IsTrackedDeviceConnected(deviceIndex);

            return isConnected;
        }
    }
}
