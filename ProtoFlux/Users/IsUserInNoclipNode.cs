using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace FrooxEngine.ProtoFlux.Locomotion
{
    [ContinuouslyChanging]
    [NodeCategory("ProtoFlux/Obsidian/Locomotion")]
    public class IsUserInNoClipNode : ValueFunctionNode<ExecutionContext, bool>
    {
        public readonly ObjectInput<User> User;

        protected override bool Compute(ExecutionContext context)
        {
            User user = User.Evaluate(context);
            if (user == null)
            {
                return false;
            }

            LocomotionController locomotionController = user.Root?.GetRegisteredComponent<LocomotionController>();
            if (locomotionController == null)
            {
                return false;
            }

            return locomotionController.ActiveModule.GetType() == typeof(NoclipLocomotion);
        }
    }
}
