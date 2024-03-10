using Valve.VR;

namespace OpenvrDataGetter
{
    class ClassOfIndex : TrackedDeviceData<ETrackedDeviceClass>
    {
        public override ETrackedDeviceClass Content => OpenVR.System.GetTrackedDeviceClass(Index.Evaluate());
    }
}
