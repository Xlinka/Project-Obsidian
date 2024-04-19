using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Locomotion
{
    [ContinuouslyChanging]
    [NodeCategory("Obsidian/Locomotion")]
    public class IsUserInSeatedModeNode : ValueFunctionNode<FrooxEngineContext, bool>
    {
        public readonly ObjectInput<User> User;

        protected override bool Compute(FrooxEngineContext context)
        {
            var user = User.Evaluate(context);
            return user != null && user.InputInterface.SeatedMode;
        }
    }
}