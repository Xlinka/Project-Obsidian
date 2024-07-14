using System.Linq;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using Obsidian.Elements;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeName("Count")]
    [NodeCategory("Obsidian/Json")]
    public class JsonCountArrayChildrenNode : ValueFunctionNode<FrooxEngineContext, int>
    {
        public readonly ObjectInput<JsonArray> Input;
        
        protected override int Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            return input?.Count ?? -1;
        }
    }
}
