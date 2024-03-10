using System;

namespace OpenvrDataGetter
{
    public abstract class DevicePropertyArray<T, P> : DevicePropertyArrayBase<T, P, T> where T : unmanaged where P : Enum
    {
    }
}
