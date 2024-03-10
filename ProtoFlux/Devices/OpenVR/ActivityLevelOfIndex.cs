using Valve.VR;

namespace OpenvrDataGetter
{
    class ActivityLevelOfIndex : TrackedDeviceData<EDeviceActivityLevel>
    {
        public override EDeviceActivityLevel Content => OpenVR.System.GetTrackedDeviceActivityLevel(Index.Evaluate());
    }
}