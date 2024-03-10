using ProtoFlux.Core;
using Valve.VR;
using System;
using System.Runtime.InteropServices;
using FrooxEngine;

namespace OpenvrDataGetter
{
    public abstract class DevicePropertyArrayBase<T, P, R> : DeviceProperty<R, P> where T : unmanaged where P : Enum
    {
        public readonly ValueInput<uint> ArrIndex;
        static protected P DefaultValue = (P)Enum.GetValues(typeof(P)).GetValue(0);
        int structSize = Marshal.SizeOf<T>();
        static protected uint trueIndexFactor = 1;
        protected virtual R Reader(T[] apiVal, uint arrindex) => (R)(object)apiVal[arrindex];
        public override R Content
        {
            get
            {
                var arrindex = ArrIndex.Evaluate();
                if (arrindex == uint.MaxValue) return default(R);
                var length = (arrindex/trueIndexFactor) + 1;
                var memsize = length * structSize;
                if (memsize >= uint.MaxValue) return default(R);
                var devindex = Index.Evaluate();
                var prop = (ETrackedDeviceProperty)(object)base.prop.Evaluate(DefaultValue);
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
}
