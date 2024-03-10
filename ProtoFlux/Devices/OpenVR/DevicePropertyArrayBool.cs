using Valve.VR;

namespace OpenvrDataGetter
{
    public class DevicePropertyArrayBool : DevicePropertyArrayBase<byte, BoolArrayDeviceProperty, bool>
    {
        protected override bool Reader(byte[] apiVal, uint arrindex)
        {
            return (apiVal[arrindex / 8] & (byte)(1 << (int)(arrindex % 8))) != 0;
        }
        static DevicePropertyArrayBool()
        {
            trueIndexFactor = 8;
        }
    }

    public enum BoolArrayDeviceProperty
    {
        Prop_DisplayMCImageData = ETrackedDeviceProperty.Prop_DisplayMCImageData_Binary,
        Prop_DisplayHiddenArea_Start = ETrackedDeviceProperty.Prop_DisplayHiddenArea_Binary_Start,
        Prop_DisplayHiddenArea_End = ETrackedDeviceProperty.Prop_DisplayHiddenArea_Binary_End,
    }
}
