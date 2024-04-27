using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Obsidian;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices;

[NodeCategory("Obsidian/Devices")]
public class ViveTrackerBattery : VoidNode<FrooxEngineContext>
{
    public ObjectArgument<User> User;
    public ValueArgument<BodyNode> BodyNode;

    [ContinuouslyChanging]
    public readonly ValueOutput<bool> IsActive;

    [ContinuouslyChanging]
    public readonly ValueOutput<float> BatteryLevel;

    [ContinuouslyChanging]
    public readonly ValueOutput<bool> IsBatteryCharging;

    protected override void ComputeOutputs(FrooxEngineContext context)
    {
        User user = 0.ReadObject<User>(context);
        if (user != null && user.IsRemoved)
        {
            user = null;
        }
        var node = 1.ReadValue<BodyNode>(context);
        var trackerDevice = user?.GetComponent<ViveTrackerProxy>(p => p.TrackerBodyNode == node);
        if (trackerDevice == null && user != null)
        {
            trackerDevice = user.AttachComponent<ViveTrackerProxy>();
            trackerDevice.TrackerBodyNode.Value = node; 
        }
        IsActive.Write(trackerDevice?.IsTrackerActive.Value ?? false, context);
        BatteryLevel.Write(trackerDevice?.BatteryLevel.Target?.Value ?? (-1f), context);
        IsBatteryCharging.Write((trackerDevice?.BatteryCharging.Target?.Value).GetValueOrDefault(), context);
    }

    public ViveTrackerBattery()
    {
        IsActive = new ValueOutput<bool>(this);
        BatteryLevel = new ValueOutput<float>(this);
        IsBatteryCharging = new ValueOutput<bool>(this);
    }
}
