using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using System;
using System.Runtime.InteropServices;
using Valve.VR;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.VR;

public abstract class DevicePropertyArrayNode<T, P, R> : ValueFunctionNode<ExecutionContext, R> where R : unmanaged where T : unmanaged where P : Enum
{
    public ValueInput<uint> DeviceIndex;
    public ValueInput<uint> ArrayIndex;
    protected static readonly P DefaultValue = (P)Enum.GetValues(typeof(P)).GetValue(0);
    protected static readonly int StructSize = Marshal.SizeOf<T>();
    protected static uint TrueIndexFactor = 1;

    // Generic reader method, to be overridden by concrete implementations
    protected virtual R Reader(T[] apiVal, uint arrindex) => (R)(object)apiVal[arrindex];

    protected override R Compute(ExecutionContext context)
    {
        var deviceIndex = DeviceIndex.Evaluate(context);
        var arrIndex = ArrayIndex.Evaluate(context);

        if (arrIndex == uint.MaxValue) return default;

        var length = (arrIndex / TrueIndexFactor) + 1;
        var memSize = length * StructSize;

        if (memSize >= uint.MaxValue) return default;

        ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;

        var arr = new T[length];

        unsafe
        {
            fixed (T* ptr = arr)
            {
                OpenVR.System?.GetArrayTrackedDeviceProperty(deviceIndex, (ETrackedDeviceProperty)(object)DefaultValue, 0, (IntPtr)ptr, (uint)memSize, ref error);
            }
        }

        return Reader(arr, arrIndex);
    }
}
