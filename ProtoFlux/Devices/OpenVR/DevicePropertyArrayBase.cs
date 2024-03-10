using ProtoFlux.Core;
using Valve.VR;
using System;
using System.Runtime.InteropServices;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Runtimes.Execution;

namespace OpenvrDataGetter
{
    public abstract class DevicePropertyArrayBase<T, P, R, C> : DeviceProperty<R, P> where T : unmanaged where P : Enum
    {
        public readonly ValueInput<uint> ArrIndex;
        protected static P DefaultValue = (P)Enum.GetValues(typeof(P)).GetValue(0);
        protected int structSize = Marshal.SizeOf<T>();
        protected static uint trueIndexFactor = 1;

        protected abstract R Reader(T[] apiVal, uint arrindex);

        public R Content(ExecutionContext context)
        {
            var arrindex = ArrIndex.Evaluate(context);
            if (arrindex == uint.MaxValue) return default(R);
            var length = (arrindex / trueIndexFactor) + 1;
            var memsize = length * structSize;
            if (memsize >= uint.MaxValue) return default(R);

            var devindex = Index.Evaluate(context);
            var prop = (ETrackedDeviceProperty)(object)base.prop.Evaluate(context, DefaultValue);
            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;

            var arr = new T[length];
            unsafe
            {
                fixed (T* ptr = arr)
                {
                    OpenVR.System.GetArrayTrackedDeviceProperty(devindex, prop, 0, (IntPtr)ptr, (uint)memsize, ref error);
                }
            }
            return Reader(arr, arrindex);
        }
    }
}
