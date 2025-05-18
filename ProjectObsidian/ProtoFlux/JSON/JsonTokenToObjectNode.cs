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

[NodeName("Token To Object")]
[NodeCategory("Obsidian/Json")]
public class JsonTokenToObjectNode : ObjectFunctionNode<FrooxEngineContext, JsonObject>
{
    public readonly ObjectInput<JsonToken> JsonToken;
    protected override JsonObject Compute(FrooxEngineContext context)
    {
        var json = JsonToken.Evaluate(context);

        if (json is null) return null;

        if (json.Wrapped is JObject obj) return new JsonObject(obj);

        return null;
    }
}