using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using FrooxEngine;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Users
{
    [ContinuouslyChanging]
    [NodeCategory("Obsidian/Users")]
    [NodeName("User From UserRef")]
    public class UserFromUserRef : ObjectFunctionNode<FrooxEngineContext, User>
    {
        public readonly ObjectInput<UserRef> UserRef;

        protected override User Compute(FrooxEngineContext context)
        {
            if (UserRef.Evaluate(context) == null)
            {
                return null;
            }
            UserRef userRef = UserRef.Evaluate(context);
            return userRef.Target;
        }
    }
}