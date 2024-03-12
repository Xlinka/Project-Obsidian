using System;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.VR;

namespace OpenvrDataGetter.Nodes;

public abstract class DevicePropertyArray<T, P> : DevicePropertyArrayBase<T, P, T> where T : unmanaged where P : unmanaged, Enum
{
}