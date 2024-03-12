using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.VR
{
    [NodeCategory("Obsidian/VR")]
    public class VRDeviceActivityLevelNode : ValueFunctionNode<ExecutionContext, EDeviceActivityLevel>
    {
        public ValueInput<int> DeviceIndex;

        protected override EDeviceActivityLevel Compute(ExecutionContext context)
        {
            var index = DeviceIndex.Evaluate(context);
            try
            {
                if (OpenVR.System != null && index >= 0)
                {
                    return OpenVR.System.GetTrackedDeviceActivityLevel((uint)index);
                }
                else
                {
                    throw new InvalidOperationException("OpenVR not initialized or invalid device index.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get VR device activity level: {ex.Message}");
            }
        }

    }
}
