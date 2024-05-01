﻿using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace FrooxEngine.ProtoFlux.Status
{
    [ContinuouslyChanging]
    [NodeCategory("ProtoFlux/Obsidian/Status")]
    public class IsUserInSeatedModeNode : ValueFunctionNode<ExecutionContext, bool>
    {
        public readonly ObjectInput<User> User;

        protected override bool Compute(ExecutionContext context)
        {
            User user = User.Evaluate(context);
            return user == null ? false : user.InputInterface.SeatedMode;
        }
    }
}