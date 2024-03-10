using BaseX;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyMatrix3x4 : DeviceProperty<float4x4, Matrix3x4DeviceProperty>
    {
        public override float4x4 Content
        {
            get
            {
                ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
                return Converter.HmdMatrix34ToFloat4x4(OpenVR.System.GetMatrix34TrackedDeviceProperty(Index.Evaluate(), (ETrackedDeviceProperty)prop.Evaluate(), ref error));
            }
        }
    }
    public enum Matrix3x4DeviceProperty
    {
        Prop_StatusDisplayTransform = ETrackedDeviceProperty.Prop_StatusDisplayTransform_Matrix34,
        Prop_CameraToHeadTransform = ETrackedDeviceProperty.Prop_CameraToHeadTransform_Matrix34,
        Prop_ImuToHeadTransform = ETrackedDeviceProperty.Prop_ImuToHeadTransform_Matrix34
    }
}
