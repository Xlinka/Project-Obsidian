using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Users.Avatar
{
    [ContinuouslyChanging]
    [NodeCategory("ProtoFlux/Obsidian/Slots")]
    public class CreateEmptySlot : ActionBreakableFlowNode<FrooxEngineContext>
    {
        public readonly ObjectInput<Slot> Parent;
        public readonly ObjectInput<string> Name;
        public readonly ObjectInput<string> Tag;
        public readonly ValueInput<bool> Persistent;
        public readonly ValueInput<bool> Active;

        public readonly ObjectOutput<Slot> Slot;

        protected override bool Do(FrooxEngineContext context)
        {
            Slot slot = context.LocalUser.LocalUserSpace.AddSlot();
            slot.Parent = Parent.Evaluate(context);
            slot.Name = Name.Evaluate(context);
            slot.Tag = Tag.Evaluate(context);
            slot.PersistentSelf = Persistent.Evaluate(context);
            slot.ActiveSelf = Active.Evaluate(context);

            Slot.Write(slot, context);
            return true;
        }
    }
}