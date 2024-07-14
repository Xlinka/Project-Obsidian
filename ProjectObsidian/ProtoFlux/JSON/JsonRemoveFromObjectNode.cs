using System;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using Obsidian.Elements;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeName("Remove From Object")]
[NodeCategory("Obsidian/Json")]
public class JsonRemoveFromObjectNode : ObjectFunctionNode<FrooxEngineContext, JsonObject>
{
    public readonly ObjectInput<JsonObject> Input;
    public readonly ObjectInput<string> Tag;

  
    protected override JsonObject Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        if (input == null) return null;

        var tag = Tag.Evaluate(context);
        return string.IsNullOrEmpty(tag) ? input : input.Remove(tag);
    }
}