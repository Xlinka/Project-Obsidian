using System;

namespace OpenvrDataGetter.Nodes;

public abstract class DevicePropertyArray<T, P> : DevicePropertyArrayBase<T, P, T> where T : unmanaged where P : unmanaged, Enum
{
}