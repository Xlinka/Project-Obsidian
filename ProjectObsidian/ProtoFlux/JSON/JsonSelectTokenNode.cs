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

[NodeName("Select Token")]
[NodeCategory("Obsidian/Json")]
public class JsonSelectTokenNode : ObjectFunctionNode<FrooxEngineContext, string>
{
    public readonly ObjectInput<JsonObject> JsonObject;
    public readonly ObjectInput<string> Path;
    protected override string Compute(FrooxEngineContext context)
    {
        var json = JsonObject.Evaluate(context);
        var path = Path.Evaluate(context);

        if (json is null || string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            JToken thing = json.Wrapped.SelectToken(path);
            return thing.ToString();
        }
        catch (Exception ex)
        {
            UniLog.Error($"Error in json path node: {ex}");
            return null;
        }
    }
}