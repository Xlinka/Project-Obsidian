using OpenvrDataGetter.Nodes;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.VR;
using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyArrayBool : DevicePropertyArrayBase<byte, BoolArrayDeviceProperty, bool>
    {
        protected override bool Reader(byte[] apiVal, uint arrindex)
        {
            // Convert the byte array to a boolean value based on the bit position
            return (apiVal[arrindex / 8] & (byte)(1 << (int)(arrindex % 8))) != 0;
        }

        static DevicePropertyArrayBool()
        {
            // Assuming this static constructor sets a class-level property or performs necessary initialization
            TrueIndexFactor = 8; // Adjust or remove based on actual usage and requirements
        }
    }

    // Enum for defining boolean array device properties, mapping them to OpenVR tracked device properties
    public enum BoolArrayDeviceProperty
    {
        Prop_DisplayMCImageData = ETrackedDeviceProperty.Prop_DisplayMCImageData_Binary,
        Prop_DisplayHiddenArea_Start = ETrackedDeviceProperty.Prop_DisplayHiddenArea_Binary_Start,
        Prop_DisplayHiddenArea_End = ETrackedDeviceProperty.Prop_DisplayHiddenArea_Binary_End,
    }
}
