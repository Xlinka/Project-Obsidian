using OpenvrDataGetter.Nodes;
using Valve.VR;
using ProtoFlux.Runtimes.Execution;

namespace OpenvrDataGetter
{
    public class RoleOfIndex : TrackedDeviceData<ETrackedControllerRole>
    {
        protected override ETrackedControllerRole Compute(ExecutionContext context)
        {
            uint deviceIndex = Index.Evaluate(context);

            ETrackedControllerRole role = OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex);

            return role;
        }
    }
}
