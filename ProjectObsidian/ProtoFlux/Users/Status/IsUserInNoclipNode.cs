using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Locomotion
{
    [ContinuouslyChanging]
    [NodeCategory("Obsidian/Locomotion")]
    public class IsUserInNoClipNode : ValueFunctionNode<FrooxEngineContext, bool>
    {
        public readonly ObjectInput<User> User;

        protected override bool Compute(FrooxEngineContext context)
        {
            var user = User.Evaluate(context);

            var locomotionController = user?.Root?.GetRegisteredComponent<LocomotionController>();
            if (locomotionController == null) return false;
            return locomotionController.ActiveModule.GetType() == typeof(NoclipLocomotion);
        }
    }
}
