using FrooxEngine.LogiX;
using FrooxEngine;

namespace OpenvrDataGetter
{
    [Category(new string[] { "LogiX/Add-Ons/OpenvrDataGetter" })]
    public abstract class TrackedDeviceData<T> : LogixOperator<T>
    {
        public readonly Input<uint> Index;
    }
}
