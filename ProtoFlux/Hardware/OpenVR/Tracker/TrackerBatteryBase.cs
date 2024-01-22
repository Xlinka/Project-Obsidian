using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace Obsidian
{
    public abstract class TrackerBatteryBase : VoidNode<FrooxEngineContext>
    {
        public ObjectArgument<User> User;
        public readonly ValueOutput<float> BatteryLevel;
        public readonly ValueOutput<bool> IsBatteryCharging;

        private ViveTracker _lastTracker;
        private SyncRef<ValueStream<float>> _batteryLevelStream;
        private SyncRef<ValueStream<bool>> _batteryChargingStream;

        protected abstract ViveTracker GetViveTracker();

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            User user = 0.ReadObject<User>(context);
            if (user != null && user.IsRemoved)
            {
                user = null;
            }
            ViveTracker device = GetViveTracker();
            if (device != _lastTracker)
            {
                _batteryLevelStream.Target = device?.BatteryLevel.GetStream(context.World);
                _batteryChargingStream.Target = device?.BatteryCharging.GetStream(context.World);
                _lastTracker = device;
            }

            BatteryLevel.Write(_batteryLevelStream.Target?.Value ?? -1f, context);
            IsBatteryCharging.Write(_batteryChargingStream.Target?.Value ?? false, context);
        }

        public TrackerBatteryBase()
        {
            BatteryLevel = new ValueOutput<float>(this);
            IsBatteryCharging = new ValueOutput<bool>(this);
        }
    }
}
