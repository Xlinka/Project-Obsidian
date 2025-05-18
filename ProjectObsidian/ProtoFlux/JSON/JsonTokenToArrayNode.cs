using System.Linq;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Core;
using Newtonsoft.Json.Linq;
using System;
using Elements.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeName("Token To Array")]
[NodeCategory("Obsidian/Json")]
public class JsonTokenToArrayNode : ObjectFunctionNode<FrooxEngineContext, JsonArray>
{
    public readonly ObjectInput<JsonToken> JsonToken;
    protected override JsonArray Compute(FrooxEngineContext context)
    {
        var json = JsonToken.Evaluate(context);

        if (json is null) return null;

        if (json.Wrapped is JArray arr) return new JsonArray(arr);

        return null;
    }
}