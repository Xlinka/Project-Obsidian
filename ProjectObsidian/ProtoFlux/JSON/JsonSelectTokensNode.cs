using System.Linq;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Core;
using Newtonsoft.Json.Linq;
using System;
using Elements.Core;
using System.Collections.Generic;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeName("Select Tokens")]
[NodeCategory("Obsidian/Json")]
public class JsonSelectTokensNode : ObjectFunctionNode<FrooxEngineContext, JsonArray>
{
    public readonly ObjectInput<JsonObject> JsonObject;
    public readonly ObjectInput<string> Path;
    protected override JsonArray Compute(FrooxEngineContext context)
    {
        var json = JsonObject.Evaluate(context);
        var path = Path.Evaluate(context);

        if (json is null || string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            IEnumerable<JToken> thing = json.Wrapped.SelectTokens(path);
            JArray array = new JArray(thing);
            return new JsonArray(array);
        }
        catch (Exception ex)
        {
            UniLog.Error($"Error in json select tokens node: {ex}");
            return null;
        }
    }
}