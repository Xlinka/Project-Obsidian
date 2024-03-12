using System;
using Elements.Core;
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
                // Ensure OpenVR is initialized and the index is valid before fetching the activity level
                if (OpenVR.System != null && index >= 0)
                {
                    return OpenVR.System.GetTrackedDeviceActivityLevel((uint)index);
                }
                else
                {
                    // Log or handle the case where OpenVR is not initialized or the index is invalid
                    UniLog.Log("OpenVR not initialized or invalid device index.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, possibly logging them or defaulting to a safe return value
                throw new Exception($"Failed to get VR device activity level: {ex.Message}");
            }
        }
    }
}
