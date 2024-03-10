using Valve.VR;

namespace OpenvrDataGetter
{
    class IsIndexConnected : TrackedDeviceData<bool>
    {
        public override bool Content => OpenVR.System.IsTrackedDeviceConnected(Index.Evaluate());
    }
}
