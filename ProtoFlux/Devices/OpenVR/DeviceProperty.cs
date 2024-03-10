using ProtoFlux.Core;
using System;

namespace OpenvrDataGetter
{
    public abstract class DeviceProperty<T, P> : TrackedDeviceData<T> where P : Enum
    {
        public readonly ObjectInput<P> prop = new ObjectInput<P>();

      
    }
}
