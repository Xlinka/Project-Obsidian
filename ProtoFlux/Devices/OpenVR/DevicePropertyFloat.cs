using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyFloat : DeviceProperty<float, FloatDeviceProperty>
    {
        public override float Content
        {
            get
            {
                ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
                return OpenVR.System.GetFloatTrackedDeviceProperty(Index.Evaluate(), (ETrackedDeviceProperty)prop.Evaluate(), ref error);
            }
        }
    }
    public enum FloatDeviceProperty
    {
        Prop_DeviceBatteryPercentage = ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float,
        Prop_SecondsFromVsyncToPhotons = ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float,
        Prop_DisplayFrequency = ETrackedDeviceProperty.Prop_DisplayFrequency_Float,
        Prop_UserIpdMeters = ETrackedDeviceProperty.Prop_UserIpdMeters_Float,
        Prop_DisplayMCOffset = ETrackedDeviceProperty.Prop_DisplayMCOffset_Float,
        Prop_DisplayMCScale = ETrackedDeviceProperty.Prop_DisplayMCScale_Float,
        Prop_DisplayGCBlackClamp = ETrackedDeviceProperty.Prop_DisplayGCBlackClamp_Float,
        Prop_DisplayGCOffset = ETrackedDeviceProperty.Prop_DisplayGCOffset_Float,
        Prop_DisplayGCScale = ETrackedDeviceProperty.Prop_DisplayGCScale_Float,
        Prop_DisplayGCPrescale = ETrackedDeviceProperty.Prop_DisplayGCPrescale_Float,
        Prop_LensCenterLeftU = ETrackedDeviceProperty.Prop_LensCenterLeftU_Float,
        Prop_LensCenterLeftV = ETrackedDeviceProperty.Prop_LensCenterLeftV_Float,
        Prop_LensCenterRightU = ETrackedDeviceProperty.Prop_LensCenterRightU_Float,
        Prop_LensCenterRightV = ETrackedDeviceProperty.Prop_LensCenterRightV_Float,
        Prop_UserHeadToEyeDepthMeters = ETrackedDeviceProperty.Prop_UserHeadToEyeDepthMeters_Float,
        Prop_ScreenshotHorizontalFieldOfViewDegrees = ETrackedDeviceProperty.Prop_ScreenshotHorizontalFieldOfViewDegrees_Float,
        Prop_ScreenshotVerticalFieldOfViewDegrees = ETrackedDeviceProperty.Prop_ScreenshotVerticalFieldOfViewDegrees_Float,
        Prop_SecondsFromPhotonsToVblank = ETrackedDeviceProperty.Prop_SecondsFromPhotonsToVblank_Float,
        Prop_MinimumIpdStepMeters = ETrackedDeviceProperty.Prop_MinimumIpdStepMeters_Float,
        Prop_DisplayMinAnalogGain = ETrackedDeviceProperty.Prop_DisplayMinAnalogGain_Float,
        Prop_DisplayMaxAnalogGain = ETrackedDeviceProperty.Prop_DisplayMaxAnalogGain_Float,
        Prop_DashboardScale = ETrackedDeviceProperty.Prop_DashboardScale_Float,
        Prop_IpdUIRangeMinMeters = ETrackedDeviceProperty.Prop_IpdUIRangeMinMeters_Float,
        Prop_IpdUIRangeMaxMeters = ETrackedDeviceProperty.Prop_IpdUIRangeMaxMeters_Float,
        Prop_Audio_DefaultPlaybackDeviceVolume = ETrackedDeviceProperty.Prop_Audio_DefaultPlaybackDeviceVolume_Float,
        Prop_FieldOfViewLeftDegrees = ETrackedDeviceProperty.Prop_FieldOfViewLeftDegrees_Float,
        Prop_FieldOfViewRightDegrees = ETrackedDeviceProperty.Prop_FieldOfViewRightDegrees_Float,
        Prop_FieldOfViewTopDegrees = ETrackedDeviceProperty.Prop_FieldOfViewTopDegrees_Float,
        Prop_FieldOfViewBottomDegrees = ETrackedDeviceProperty.Prop_FieldOfViewBottomDegrees_Float,
        Prop_TrackingRangeMinimumMeters = ETrackedDeviceProperty.Prop_TrackingRangeMinimumMeters_Float,
        Prop_TrackingRangeMaximumMeters = ETrackedDeviceProperty.Prop_TrackingRangeMaximumMeters_Float
    }
}
