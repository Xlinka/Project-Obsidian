using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyArrayInt : DevicePropertyArray<int, IntArrayDeviceProperty>
    {
    }

    public enum IntArrayDeviceProperty
    {
        Prop_CameraDistortionFunction = ETrackedDeviceProperty.Prop_CameraDistortionFunction_Int32_Array
    }
}
