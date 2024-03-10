using System;
using ProtoFlux.Core;

namespace OpenvrDataGetter
{
    public abstract class DeviceProperty<T, P> : TrackedDeviceData<T> where P : Enum
    {
        public readonly ObjectInput<P> prop;
    }
}
