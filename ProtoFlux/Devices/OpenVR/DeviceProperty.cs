using FrooxEngine.LogiX;
using System;

namespace OpenvrDataGetter
{
    public abstract class DeviceProperty<T, P> : TrackedDeviceData<T> where P : Enum
    {
        public readonly Input<P> Prop;
    }
}
