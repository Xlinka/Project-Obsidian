using ProtoFlux.Core;
using System;

namespace OpenvrDataGetter.Nodes
{
    public abstract class DeviceProperty<T, P> : TrackedDeviceData<T> where P : Enum
    {
        public ObjectInput<P> Prop;

    }
}
