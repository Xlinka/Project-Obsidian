using System;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using Obsidian.Elements;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonParseStringArrayNode : ObjectFunctionNode<FrooxEngineContext, JsonArray>
    {
        public readonly ObjectInput<string> Input;

        protected override JsonArray Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            return string.IsNullOrEmpty(input) ? null : JsonArray.FromString(input);
        }
    }
}
