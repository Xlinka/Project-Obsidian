using Elements.Core;
using FrooxEngine;
using ProtoFlux.Runtimes.Execution;
using System;
using Valve.VR;

namespace OpenvrDataGetter.Nodes
{
    public class DevicePropertyFloat3 : DeviceProperty<float3, Float3DeviceProperty>
    {
        protected override float3 Compute(ExecutionContext context)
        {
            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
            var Float3 = new float3[1];
            uint deviceIndex = Index.Evaluate(context); 
            Float3DeviceProperty prop = Prop.Evaluate(context); 

            unsafe
            {
                fixed (float3* pFloat3 = Float3)
                {
                    OpenVR.System.GetArrayTrackedDeviceProperty(deviceIndex, (ETrackedDeviceProperty)prop, 0, (IntPtr)pFloat3, (uint)sizeof(float3), ref error);
                }
            }

            if (error != ETrackedPropertyError.TrackedProp_Success)
            {
                throw new InvalidOperationException($"Failed to get float3 device property. Error: {error}");
            }

            return Float3[0]; 
        }
    }

    public enum Float3DeviceProperty
    {
        Prop_ImuFactoryGyroBias = ETrackedDeviceProperty.Prop_ImuFactoryGyroBias_Vector3,
        Prop_ImuFactoryGyroScale = ETrackedDeviceProperty.Prop_ImuFactoryGyroScale_Vector3,
        Prop_ImuFactoryAccelerometerBias = ETrackedDeviceProperty.Prop_ImuFactoryAccelerometerBias_Vector3,
        Prop_ImuFactoryAccelerometerScale = ETrackedDeviceProperty.Prop_ImuFactoryAccelerometerScale_Vector3,
        Prop_DisplayColorMultLeft = ETrackedDeviceProperty.Prop_DisplayColorMultLeft_Vector3,
        Prop_DisplayColorMultRight = ETrackedDeviceProperty.Prop_DisplayColorMultRight_Vector3
    }
}
