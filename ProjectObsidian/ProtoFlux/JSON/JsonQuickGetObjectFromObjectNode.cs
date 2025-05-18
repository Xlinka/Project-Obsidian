using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeName("Quick Get From Object")]
[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(string), typeof(Uri), typeof(JsonObject), typeof(JsonArray), typeof(JsonToken))]
public class JsonQuickGetObjectFromObjectNode<T> : ObjectFunctionNode<FrooxEngineContext, T> where T : class
{
    public readonly ObjectInput<string> Input;
    public readonly ObjectInput<string> Tag;
    public static bool IsValidGenericType => JsonTypeHelper.ValidObjectGetTypes.Contains(typeof(T));
    protected override T Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        var tag = Tag.Evaluate(context);
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(tag))
            return default;
        var from = JsonObject.FromString(input);
        return from?.GetObject<T>(tag);
    }
}