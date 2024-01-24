using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("ProtoFlux/Obsidian/Json")]
    public class JsonEmptyObjectNode : ObjectFunctionNode<ExecutionContext, JObject>
    {
        protected override JObject Compute(ExecutionContext context)
        {
            return new JObject();
        }
    }

    [NodeCategory("ProtoFlux/Obsidian/Json")]
    public class JsonEmptyArrayNode : ObjectFunctionNode<ExecutionContext, JArray>
    {
        protected override JArray Compute(ExecutionContext context)
        {
            return new JArray();
        }
    }
    [NodeCategory("ProtoFlux/Obsidian/Json")]
    public class JsonNullValueNode : ObjectFunctionNode<ExecutionContext, JToken>
    {
        protected override JToken Compute(ExecutionContext context)
        {
            return JValue.CreateNull();
        }
    }
}