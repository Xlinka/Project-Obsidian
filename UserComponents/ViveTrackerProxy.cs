using System.Linq;
using FrooxEngine;

namespace Obsidian;

public class ViveTrackerProxy : UserComponent
{
    public readonly Sync<BodyNode> TrackerBodyNode;

    public readonly Sync<bool> IsTrackerActive;

    public readonly SyncRef<ValueStream<float>> BatteryLevel;

    public readonly SyncRef<ValueStream<bool>> BatteryCharging;

    private ViveTracker _currentTracker;

    protected override void OnCommonUpdate()
    {
        if (base.User.IsLocalUser)
        {
            ViveTracker device = InputInterface.GetDevices<ViveTracker>().FirstOrDefault(t => t.CorrespondingBodyNode == TrackerBodyNode.Value);
            if (device != _currentTracker)
            {
                TrackerBodyNode.Value = device?.CorrespondingBodyNode ?? BodyNode.NONE;
                BatteryLevel.Target = device?.BatteryLevel.GetStream(base.World);
                BatteryCharging.Target = device?.BatteryCharging.GetStream(base.World);
                IsTrackerActive.Value = device != null;
                _currentTracker = device;
            }
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        _currentTracker = null;
    }
}
