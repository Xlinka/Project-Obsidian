using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using Obsidian.Elements;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeCategory("Obsidian/Json")]
public class JsonEmptyObjectNode : ObjectFunctionNode<FrooxEngineContext, JsonObject>
{
    protected override JsonObject Compute(FrooxEngineContext context) => new(new JObject());
}

[NodeCategory("Obsidian/Json")]
public class JsonEmptyArrayNode : ObjectFunctionNode<FrooxEngineContext, JsonArray>
{
    protected override JsonArray Compute(FrooxEngineContext context) => new(new JArray());
}