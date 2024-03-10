using BaseX;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyArrayFloat4 : DevicePropertyArray<float4, Float4ArrayDeviceProperty>
    {
    }

    public enum Float4ArrayDeviceProperty
    {
        Prop_CameraWhiteBalance = ETrackedDeviceProperty.Prop_CameraWhiteBalance_Vector4_Array
    }
}
