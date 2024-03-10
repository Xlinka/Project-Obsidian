using ProtoFlux.Core;
using FrooxEngine;
using Valve.VR;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Runtimes.Execution;

namespace OpenvrDataGetter
{
    public class DevicePropertyUlong : ValueFunctionNode<FrooxEngineContext, ulong>
    {
        public ValueInput<uint> Index;
        public ValueInput<UlongDeviceProperty> prop;

        protected override ulong Compute(FrooxEngineContext context)
        {
            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
            return OpenVR.System.GetUint64TrackedDeviceProperty(Index.Evaluate(context), (ETrackedDeviceProperty)prop.Evaluate(context), ref error);
        }
    }

    public enum UlongDeviceProperty
    {
        Prop_HardwareRevision = ETrackedDeviceProperty.Prop_HardwareRevision_Uint64,
        Prop_FirmwareVersion = ETrackedDeviceProperty.Prop_FirmwareVersion_Uint64,
        Prop_FPGAVersion = ETrackedDeviceProperty.Prop_FPGAVersion_Uint64,
        Prop_VRCVersion = ETrackedDeviceProperty.Prop_VRCVersion_Uint64,
        Prop_RadioVersion = ETrackedDeviceProperty.Prop_RadioVersion_Uint64,
        Prop_DongleVersion = ETrackedDeviceProperty.Prop_DongleVersion_Uint64,
        Prop_ParentDriver = ETrackedDeviceProperty.Prop_ParentDriver_Uint64,
        Prop_BootloaderVersion = ETrackedDeviceProperty.Prop_BootloaderVersion_Uint64,
        Prop_PeripheralApplicationVersion = ETrackedDeviceProperty.Prop_PeripheralApplicationVersion_Uint64,
        Prop_CurrentUniverseId = ETrackedDeviceProperty.Prop_CurrentUniverseId_Uint64,
        Prop_PreviousUniverseId = ETrackedDeviceProperty.Prop_PreviousUniverseId_Uint64,
        Prop_DisplayFirmwareVersion = ETrackedDeviceProperty.Prop_DisplayFirmwareVersion_Uint64,
        Prop_CameraFirmwareVersion = ETrackedDeviceProperty.Prop_CameraFirmwareVersion_Uint64,
        Prop_DisplayFPGAVersion = ETrackedDeviceProperty.Prop_DisplayFPGAVersion_Uint64,
        Prop_DisplayBootloaderVersion = ETrackedDeviceProperty.Prop_DisplayBootloaderVersion_Uint64,
        Prop_DisplayHardwareVersion = ETrackedDeviceProperty.Prop_DisplayHardwareVersion_Uint64,
        Prop_AudioFirmwareVersion = ETrackedDeviceProperty.Prop_AudioFirmwareVersion_Uint64,
        Prop_GraphicsAdapterLuid = ETrackedDeviceProperty.Prop_GraphicsAdapterLuid_Uint64,
        Prop_AudioBridgeFirmwareVersion = ETrackedDeviceProperty.Prop_AudioBridgeFirmwareVersion_Uint64,
        Prop_ImageBridgeFirmwareVersion = ETrackedDeviceProperty.Prop_ImageBridgeFirmwareVersion_Uint64,
        Prop_AdditionalRadioFeatures = ETrackedDeviceProperty.Prop_AdditionalRadioFeatures_Uint64,
        Prop_SupportedButtons = ETrackedDeviceProperty.Prop_SupportedButtons_Uint64,
        Prop_OverrideContainer = ETrackedDeviceProperty.Prop_OverrideContainer_Uint64
    }
}
