using OpenvrDataGetter.Nodes;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.VR;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyArrayFloat : DevicePropertyArray<float, FloatArrayDeviceProperty>
    {
    }

    public enum FloatArrayDeviceProperty
    {
        Prop_CameraDistortionCoefficients = ETrackedDeviceProperty.Prop_CameraDistortionCoefficients_Float_Array,
        Prop_DisplayAvailableFrameRates = ETrackedDeviceProperty.Prop_DisplayAvailableFrameRates_Float_Array
    }
}
