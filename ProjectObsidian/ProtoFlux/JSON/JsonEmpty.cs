using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonEmptyObjectNode : ObjectFunctionNode<FrooxEngineContext, JObject>
    {
        protected override JObject Compute(FrooxEngineContext context)
        {
            return new JObject();
        }
    }

    [NodeCategory("Obsidian/Json")]
    public class JsonEmptyArrayNode : ObjectFunctionNode<FrooxEngineContext, JArray>
    {
        protected override JArray Compute(FrooxEngineContext context)
        {
            return new JArray();
        }
    }
    [NodeCategory("Obsidian/Json")]
    public class JsonNullValueNode : ObjectFunctionNode<FrooxEngineContext, JToken>
    {
        protected override JToken Compute(FrooxEngineContext context)
        {
            return JValue.CreateNull();
        }
    }
}