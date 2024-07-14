using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using Obsidian.Elements;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeName("Empty JsonObject")]
[NodeCategory("Obsidian/Json")]
public class JsonEmptyObjectNode : ObjectFunctionNode<FrooxEngineContext, JsonObject>
{
    protected override JsonObject Compute(FrooxEngineContext context) => new(new JObject());
}