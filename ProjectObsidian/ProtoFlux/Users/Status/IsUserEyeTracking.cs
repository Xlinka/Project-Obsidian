﻿using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Users.Status
{
    [NodeCategory("Obsidian/Users/Status")]
    [NodeName("Is User Eye Tracking")]
    public class IsUserEyeTracking : ValueFunctionNode<FrooxEngineContext, bool>
    {
        public readonly ObjectInput<User> User;
        public readonly ObjectInput<EyeSide> Side;

        protected override bool Compute(FrooxEngineContext context)
        {
            User user = User.Evaluate(context);
            if (user != null)
            {
                EyeTrackingStreamManager eyeTrackingStreamManager = user.Root.GetRegisteredComponent<EyeTrackingStreamManager>();
                if (eyeTrackingStreamManager != null)
                {
                    EyeSide side = Side.Evaluate(context);
                    return eyeTrackingStreamManager.GetIsTracking(side);
                }
            }
            return false;
        }
    }
}
