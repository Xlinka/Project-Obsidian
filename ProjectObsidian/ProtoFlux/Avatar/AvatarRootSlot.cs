using System.Collections.Generic;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Users.Avatar
{
    [ContinuouslyChanging]
    [NodeCategory("Obsidian/Avatar")]
    public class AvatarRootSlot : ObjectFunctionNode<ExecutionContext, Slot>
    {
        public readonly ObjectInput<User> User;

        protected override Slot Compute(ExecutionContext context)
        {
            User user = User.Evaluate(context);
            Slot slot = user.Root.Slot;
            List<AvatarRoot> list = new List<AvatarRoot>();
            slot.GetFirstDirectComponentsInChildren(list);
            return user == null || list.Count == 0 ? null : list[0].Slot;
        }
    }
}