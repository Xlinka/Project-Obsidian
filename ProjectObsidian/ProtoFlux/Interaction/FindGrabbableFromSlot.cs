using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Users.Avatar
{
    [ContinuouslyChanging]
    [NodeCategory("ProtoFlux/Obsidian/Avatar")]
    public class FindGrabbableFromSlot : ObjectFunctionNode<ExecutionContext, IGrabbable>
    {
        public readonly ObjectInput<Slot> Slot;

        protected override IGrabbable Compute(ExecutionContext context)
        {
            Slot slot = Slot.Evaluate(context);
            return slot == null ? null : slot.GetComponentInParents<Grabbable>();
        }
    }
}