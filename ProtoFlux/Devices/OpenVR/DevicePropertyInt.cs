using OpenvrDataGetter.Nodes;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyInt : DeviceProperty<int, IntDeviceProperty>
    {

        protected override int Compute(ExecutionContext context)
        {
            {
                ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
                return OpenVR.System.GetInt32TrackedDeviceProperty(Index.Evaluate(context), (ETrackedDeviceProperty)Prop.Evaluate(context), ref error);
            }
        }
    }
    public enum IntDeviceProperty
    {
        Prop_DeviceClass = ETrackedDeviceProperty.Prop_DeviceClass_Int32,
        Prop_NumCameras = ETrackedDeviceProperty.Prop_NumCameras_Int32,
        Prop_CameraFrameLayout = ETrackedDeviceProperty.Prop_CameraFrameLayout_Int32,
        Prop_CameraStreamFormat = ETrackedDeviceProperty.Prop_CameraStreamFormat_Int32,
        Prop_EstimatedDeviceFirstUseTime = ETrackedDeviceProperty.Prop_EstimatedDeviceFirstUseTime_Int32,
        Prop_DisplayMCType = ETrackedDeviceProperty.Prop_DisplayMCType_Int32,
        Prop_EdidVendorID = ETrackedDeviceProperty.Prop_EdidVendorID_Int32,
        Prop_EdidProductID = ETrackedDeviceProperty.Prop_EdidProductID_Int32,
        Prop_DisplayGCType = ETrackedDeviceProperty.Prop_DisplayGCType_Int32,
        Prop_CameraCompatibilityMode = ETrackedDeviceProperty.Prop_CameraCompatibilityMode_Int32,
        Prop_DisplayMCImageWidth = ETrackedDeviceProperty.Prop_DisplayMCImageWidth_Int32,
        Prop_DisplayMCImageHeight = ETrackedDeviceProperty.Prop_DisplayMCImageHeight_Int32,
        Prop_DisplayMCImageNumChannels = ETrackedDeviceProperty.Prop_DisplayMCImageNumChannels_Int32,
        Prop_ExpectedTrackingReferenceCount = ETrackedDeviceProperty.Prop_ExpectedTrackingReferenceCount_Int32,
        Prop_ExpectedControllerCount = ETrackedDeviceProperty.Prop_ExpectedControllerCount_Int32,
        Prop_DistortionMeshResolution = ETrackedDeviceProperty.Prop_DistortionMeshResolution_Int32,
        Prop_HmdTrackingStyle = ETrackedDeviceProperty.Prop_HmdTrackingStyle_Int32,
        Prop_DriverRequestedMuraCorrectionMode = ETrackedDeviceProperty.Prop_DriverRequestedMuraCorrectionMode_Int32,
        Prop_DriverRequestedMuraFeather_InnerLeft = ETrackedDeviceProperty.Prop_DriverRequestedMuraFeather_InnerLeft_Int32,
        Prop_DriverRequestedMuraFeather_InnerRight = ETrackedDeviceProperty.Prop_DriverRequestedMuraFeather_InnerRight_Int32,
        Prop_DriverRequestedMuraFeather_InnerTop = ETrackedDeviceProperty.Prop_DriverRequestedMuraFeather_InnerTop_Int32,
        Prop_DriverRequestedMuraFeather_InnerBottom = ETrackedDeviceProperty.Prop_DriverRequestedMuraFeather_InnerBottom_Int32,
        Prop_DriverRequestedMuraFeather_OuterLeft = ETrackedDeviceProperty.Prop_DriverRequestedMuraFeather_OuterLeft_Int32,
        Prop_DriverRequestedMuraFeather_OuterRight = ETrackedDeviceProperty.Prop_DriverRequestedMuraFeather_OuterRight_Int32,
        Prop_DriverRequestedMuraFeather_OuterTop = ETrackedDeviceProperty.Prop_DriverRequestedMuraFeather_OuterTop_Int32,
        Prop_DriverRequestedMuraFeather_OuterBottom = ETrackedDeviceProperty.Prop_DriverRequestedMuraFeather_OuterBottom_Int32,
        Prop_Axis0Type = ETrackedDeviceProperty.Prop_Axis0Type_Int32,
        Prop_Axis1Type = ETrackedDeviceProperty.Prop_Axis1Type_Int32,
        Prop_Axis2Type = ETrackedDeviceProperty.Prop_Axis2Type_Int32,
        Prop_Axis3Type = ETrackedDeviceProperty.Prop_Axis3Type_Int32,
        Prop_Axis4Type = ETrackedDeviceProperty.Prop_Axis4Type_Int32,
        Prop_ControllerRoleHint = ETrackedDeviceProperty.Prop_ControllerRoleHint_Int32,
        Prop_Nonce = ETrackedDeviceProperty.Prop_Nonce_Int32,
        Prop_ControllerHandSelectionPriority = ETrackedDeviceProperty.Prop_ControllerHandSelectionPriority_Int32
    }
}
