using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonCountJArrayChildren : ValueFunctionNode<FrooxEngineContext, int>
    {
        public readonly ObjectInput<JArray> Input;

        protected override int Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            return input?.Count ?? -1;
        }
    }
}
