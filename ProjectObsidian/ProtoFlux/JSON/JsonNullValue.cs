using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonNullValue : ObjectFunctionNode<FrooxEngineContext, JToken>
    {
        protected override JToken Compute(FrooxEngineContext context)
        {
            return JValue.CreateNull();
        }
    }
}