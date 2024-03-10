using Valve.VR;

namespace OpenvrDataGetter
{
    class RoleOfIndex : TrackedDeviceData<ETrackedControllerRole>
    {
        public override ETrackedControllerRole Content => OpenVR.System.GetControllerRoleForTrackedDeviceIndex(Index.Evaluate());
    }
}