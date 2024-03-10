using System.Text;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyString : DeviceProperty<string, StringDeviceProperty>
    {
        public override string Content
        {
            get
            {
                uint index = Index.Evaluate();
                ETrackedDeviceProperty property = (ETrackedDeviceProperty)Prop.Evaluate();
                ETrackedPropertyError pError = ETrackedPropertyError.TrackedProp_Success;
                StringBuilder stringBuilder = new StringBuilder(64);
                uint stringTrackedDeviceProperty = OpenVR.System.GetStringTrackedDeviceProperty(index, property, null, 0u, ref pError);
                if (stringTrackedDeviceProperty > 1)
                {
                    stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
                    OpenVR.System.GetStringTrackedDeviceProperty(index, property, stringBuilder, stringTrackedDeviceProperty, ref pError);
                }
                return stringBuilder.ToString();
            }
        }
    }
    public enum StringDeviceProperty
    {
        Prop_TrackingSystemName = ETrackedDeviceProperty.Prop_TrackingSystemName_String,
        Prop_ModelNumber = ETrackedDeviceProperty.Prop_ModelNumber_String,
        Prop_SerialNumber = ETrackedDeviceProperty.Prop_SerialNumber_String,
        Prop_RenderModelName = ETrackedDeviceProperty.Prop_RenderModelName_String,
        Prop_ManufacturerName = ETrackedDeviceProperty.Prop_ManufacturerName_String,
        Prop_TrackingFirmwareVersion = ETrackedDeviceProperty.Prop_TrackingFirmwareVersion_String,
        Prop_HardwareRevision = ETrackedDeviceProperty.Prop_HardwareRevision_String,
        Prop_AllWirelessDongleDescriptions = ETrackedDeviceProperty.Prop_AllWirelessDongleDescriptions_String,
        Prop_ConnectedWirelessDongle = ETrackedDeviceProperty.Prop_ConnectedWirelessDongle_String,
        Prop_Firmware_ManualUpdateURL = ETrackedDeviceProperty.Prop_Firmware_ManualUpdateURL_String,
        Prop_Firmware_ProgrammingTarget = ETrackedDeviceProperty.Prop_Firmware_ProgrammingTarget_String,
        Prop_DriverVersion = ETrackedDeviceProperty.Prop_DriverVersion_String,
        Prop_ResourceRoot = ETrackedDeviceProperty.Prop_ResourceRoot_String,
        Prop_RegisteredDeviceType = ETrackedDeviceProperty.Prop_RegisteredDeviceType_String,
        Prop_InputProfilePath = ETrackedDeviceProperty.Prop_InputProfilePath_String,
        Prop_AdditionalDeviceSettingsPath = ETrackedDeviceProperty.Prop_AdditionalDeviceSettingsPath_String,
        Prop_AdditionalSystemReportData = ETrackedDeviceProperty.Prop_AdditionalSystemReportData_String,
        Prop_CompositeFirmwareVersion = ETrackedDeviceProperty.Prop_CompositeFirmwareVersion_String,
        Prop_ManufacturerSerialNumber = ETrackedDeviceProperty.Prop_ManufacturerSerialNumber_String,
        Prop_ComputedSerialNumber = ETrackedDeviceProperty.Prop_ComputedSerialNumber_String,
        Prop_DisplayMCImageLeft = ETrackedDeviceProperty.Prop_DisplayMCImageLeft_String,
        Prop_DisplayMCImageRight = ETrackedDeviceProperty.Prop_DisplayMCImageRight_String,
        Prop_DisplayGCImage = ETrackedDeviceProperty.Prop_DisplayGCImage_String,
        Prop_CameraFirmwareDescription = ETrackedDeviceProperty.Prop_CameraFirmwareDescription_String,
        Prop_DriverProvidedChaperonePath = ETrackedDeviceProperty.Prop_DriverProvidedChaperonePath_String,
        Prop_NamedIconPathControllerLeftDeviceOff = ETrackedDeviceProperty.Prop_NamedIconPathControllerLeftDeviceOff_String,
        Prop_NamedIconPathControllerRightDeviceOff = ETrackedDeviceProperty.Prop_NamedIconPathControllerRightDeviceOff_String,
        Prop_NamedIconPathTrackingReferenceDeviceOff = ETrackedDeviceProperty.Prop_NamedIconPathTrackingReferenceDeviceOff_String,
        Prop_ExpectedControllerType = ETrackedDeviceProperty.Prop_ExpectedControllerType_String,
        Prop_HmdColumnCorrectionSettingPrefix = ETrackedDeviceProperty.Prop_HmdColumnCorrectionSettingPrefix_String,
        Prop_Audio_DefaultPlaybackDeviceId = ETrackedDeviceProperty.Prop_Audio_DefaultPlaybackDeviceId_String,
        Prop_Audio_DefaultRecordingDeviceId = ETrackedDeviceProperty.Prop_Audio_DefaultRecordingDeviceId_String,
        Prop_AttachedDeviceId = ETrackedDeviceProperty.Prop_AttachedDeviceId_String,
        Prop_ModeLabel = ETrackedDeviceProperty.Prop_ModeLabel_String,
        Prop_IconPathName = ETrackedDeviceProperty.Prop_IconPathName_String,
        Prop_NamedIconPathDeviceOff = ETrackedDeviceProperty.Prop_NamedIconPathDeviceOff_String,
        Prop_NamedIconPathDeviceSearching = ETrackedDeviceProperty.Prop_NamedIconPathDeviceSearching_String,
        Prop_NamedIconPathDeviceSearchingAlert = ETrackedDeviceProperty.Prop_NamedIconPathDeviceSearchingAlert_String,
        Prop_NamedIconPathDeviceReady = ETrackedDeviceProperty.Prop_NamedIconPathDeviceReady_String,
        Prop_NamedIconPathDeviceReadyAlert = ETrackedDeviceProperty.Prop_NamedIconPathDeviceReadyAlert_String,
        Prop_NamedIconPathDeviceNotReady = ETrackedDeviceProperty.Prop_NamedIconPathDeviceNotReady_String,
        Prop_NamedIconPathDeviceStandby = ETrackedDeviceProperty.Prop_NamedIconPathDeviceStandby_String,
        Prop_NamedIconPathDeviceAlertLow = ETrackedDeviceProperty.Prop_NamedIconPathDeviceAlertLow_String,
        Prop_NamedIconPathDeviceStandbyAlert = ETrackedDeviceProperty.Prop_NamedIconPathDeviceStandbyAlert_String,
        Prop_UserConfigPath = ETrackedDeviceProperty.Prop_UserConfigPath_String,
        Prop_InstallPath = ETrackedDeviceProperty.Prop_InstallPath_String,
        Prop_ControllerType = ETrackedDeviceProperty.Prop_ControllerType_String
    }
}
