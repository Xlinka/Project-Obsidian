using ProtoFlux.Core;
using System;

namespace OpenvrDataGetter.Nodes;

    public abstract class DeviceProperty<T, P> : TrackedDeviceData<T> where T : unmanaged where P : unmanaged, Enum
    {
        public ValueInput<P> Prop;
    }
