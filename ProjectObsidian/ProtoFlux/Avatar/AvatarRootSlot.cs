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
            if (user == null) return null;

            Slot slot = user.Root.Slot;
            List<AvatarRoot> list = Pool.BorrowList<AvatarRoot>();
            slot.GetFirstDirectComponentsInChildren(list);
            Slot avatarRoot = list.FirstOrDefault()?.Slot;
            Pool.Return(ref list);

            return avatarRoot;
        }
    }
}
