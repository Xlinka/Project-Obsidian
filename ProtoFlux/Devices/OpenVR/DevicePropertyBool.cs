using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyBool : DeviceProperty<bool, BoolDeviceProperty>
    {
        public override bool Content
        {
            get
            {
                ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
                return OpenVR.System.GetBoolTrackedDeviceProperty(Index.Evaluate(), (ETrackedDeviceProperty)Prop.Evaluate(), ref error);
            }
        }
    }
    public enum BoolDeviceProperty
    {
        Prop_WillDriftInYaw = ETrackedDeviceProperty.Prop_WillDriftInYaw_Bool,
        Prop_DeviceIsWireless = ETrackedDeviceProperty.Prop_DeviceIsWireless_Bool,
        Prop_DeviceIsCharging = ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool,
        Prop_Firmware_UpdateAvailable = ETrackedDeviceProperty.Prop_Firmware_UpdateAvailable_Bool,
        Prop_Firmware_ManualUpdate = ETrackedDeviceProperty.Prop_Firmware_ManualUpdate_Bool,
        Prop_BlockServerShutdown = ETrackedDeviceProperty.Prop_BlockServerShutdown_Bool,
        Prop_CanUnifyCoordinateSystemWithHmd = ETrackedDeviceProperty.Prop_CanUnifyCoordinateSystemWithHmd_Bool,
        Prop_ContainsProximitySensor = ETrackedDeviceProperty.Prop_ContainsProximitySensor_Bool,
        Prop_DeviceProvidesBatteryStatus = ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool,
        Prop_DeviceCanPowerOff = ETrackedDeviceProperty.Prop_DeviceCanPowerOff_Bool,
        Prop_HasCamera = ETrackedDeviceProperty.Prop_HasCamera_Bool,
        Prop_Firmware_ForceUpdateRequired = ETrackedDeviceProperty.Prop_Firmware_ForceUpdateRequired_Bool,
        Prop_ViveSystemButtonFixRequired = ETrackedDeviceProperty.Prop_ViveSystemButtonFixRequired_Bool,
        Prop_NeverTracked = ETrackedDeviceProperty.Prop_NeverTracked_Bool,
        Prop_Identifiable = ETrackedDeviceProperty.Prop_Identifiable_Bool,
        Prop_Firmware_RemindUpdate = ETrackedDeviceProperty.Prop_Firmware_RemindUpdate_Bool,
        Prop_ReportsTimeSinceVSync = ETrackedDeviceProperty.Prop_ReportsTimeSinceVSync_Bool,
        Prop_IsOnDesktop = ETrackedDeviceProperty.Prop_IsOnDesktop_Bool,
        Prop_DisplaySuppressed = ETrackedDeviceProperty.Prop_DisplaySuppressed_Bool,
        Prop_DisplayAllowNightMode = ETrackedDeviceProperty.Prop_DisplayAllowNightMode_Bool,
        Prop_DriverDirectModeSendsVsyncEvents = ETrackedDeviceProperty.Prop_DriverDirectModeSendsVsyncEvents_Bool,
        Prop_DisplayDebugMode = ETrackedDeviceProperty.Prop_DisplayDebugMode_Bool,
        Prop_DoNotApplyPrediction = ETrackedDeviceProperty.Prop_DoNotApplyPrediction_Bool,
        Prop_DriverIsDrawingControllers = ETrackedDeviceProperty.Prop_DriverIsDrawingControllers_Bool,
        Prop_DriverRequestsApplicationPause = ETrackedDeviceProperty.Prop_DriverRequestsApplicationPause_Bool,
        Prop_DriverRequestsReducedRendering = ETrackedDeviceProperty.Prop_DriverRequestsReducedRendering_Bool,
        Prop_ConfigurationIncludesLighthouse20Features = ETrackedDeviceProperty.Prop_ConfigurationIncludesLighthouse20Features_Bool,
        Prop_DriverProvidedChaperoneVisibility = ETrackedDeviceProperty.Prop_DriverProvidedChaperoneVisibility_Bool,
        Prop_CameraSupportsCompatibilityModes = ETrackedDeviceProperty.Prop_CameraSupportsCompatibilityModes_Bool,
        Prop_SupportsRoomViewDepthProjection = ETrackedDeviceProperty.Prop_SupportsRoomViewDepthProjection_Bool,
        Prop_DisplaySupportsMultipleFramerates = ETrackedDeviceProperty.Prop_DisplaySupportsMultipleFramerates_Bool,
        Prop_DisplaySupportsRuntimeFramerateChange = ETrackedDeviceProperty.Prop_DisplaySupportsRuntimeFramerateChange_Bool,
        Prop_DisplaySupportsAnalogGain = ETrackedDeviceProperty.Prop_DisplaySupportsAnalogGain_Bool,
        Prop_Audio_SupportsDualSpeakerAndJackOutput = ETrackedDeviceProperty.Prop_Audio_SupportsDualSpeakerAndJackOutput_Bool,
        Prop_CanWirelessIdentify = ETrackedDeviceProperty.Prop_CanWirelessIdentify_Bool,
        Prop_HasDisplayComponent = ETrackedDeviceProperty.Prop_HasDisplayComponent_Bool,
        Prop_HasControllerComponent = ETrackedDeviceProperty.Prop_HasControllerComponent_Bool,
        Prop_HasCameraComponent = ETrackedDeviceProperty.Prop_HasCameraComponent_Bool,
        Prop_HasDriverDirectModeComponent = ETrackedDeviceProperty.Prop_HasDriverDirectModeComponent_Bool,
        Prop_HasVirtualDisplayComponent = ETrackedDeviceProperty.Prop_HasVirtualDisplayComponent_Bool,
        Prop_HasSpatialAnchorsSupport = ETrackedDeviceProperty.Prop_HasSpatialAnchorsSupport_Bool
    }
}
