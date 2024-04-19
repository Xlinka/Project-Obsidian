using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonCountObjectChildrenNode : ValueFunctionNode<FrooxEngineContext, int>
    {
        public readonly ObjectInput<JObject> Input;

        protected override int Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            return input?.Count ?? -1;
        }
    }
}
