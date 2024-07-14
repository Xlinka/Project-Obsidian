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

[NodeName("Get From Object")]
[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(string), typeof(Uri), typeof(JsonObject), typeof(JsonArray))]
public class JsonGetObjectFromObjectNode<T> : ObjectFunctionNode<FrooxEngineContext, T> where T : class
{
    public readonly ObjectInput<JsonObject> Input;
    public readonly ObjectInput<string> Tag;
    public static bool IsValidGenericType => JsonTypeHelper.ValidObjectGetTypes.Contains(typeof(T));
    protected override T Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        var tag = Tag.Evaluate(context);
        if (input == null || string.IsNullOrEmpty(tag))
            return default;
        return input.GetObject<T>(tag);
    }
}