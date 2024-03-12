using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using System;
using System.Runtime.InteropServices;
using Valve.VR;

namespace OpenvrDataGetter.Nodes
{
    public abstract class DevicePropertyArrayBase<T, P, R> : DeviceProperty<R, P> where T : unmanaged where P : Enum where R : unmanaged
    {
        public ValueInput<uint> ArrIndex;

        protected static P DefaultValue = (P)Enum.GetValues(typeof(P)).GetValue(0);
        protected int StructSize = Marshal.SizeOf<T>();
        protected static uint TrueIndexFactor = 1;



        protected virtual R Reader(T[] apiVal, uint arrIndex) => (R)(object)apiVal[arrIndex];

        protected override R Compute(ExecutionContext context)
        {
            var arrIndex = ArrIndex.Evaluate(context);
            if (arrIndex == uint.MaxValue) return default(R);

            var length = (arrIndex / TrueIndexFactor) + 1;
            var memSize = length * StructSize;

            if (memSize >= uint.MaxValue) return default(R);

            var devIndex = Index.Evaluate(context); // Ensure Index is defined in DeviceProperty<R, P> or its base.
            var prop = (ETrackedDeviceProperty)(object)Prop.Evaluate(context);

            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
            var arr = new T[length];

            unsafe
            {
                fixed (T* ptr = arr)
                {
                    OpenVR.System.GetArrayTrackedDeviceProperty(devIndex, prop, 0, (IntPtr)ptr, (uint)memSize, ref error);
                }
            }

            return Reader(arr, arrIndex);
        }
    }
}
