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

[NodeName("Add To Object")]
[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(string), typeof(Uri), typeof(IJsonToken), typeof(JsonObject), typeof(JsonArray))]
public class JsonAddObjectToObjectNode<T> : ObjectFunctionNode<FrooxEngineContext, JsonObject> where T : class
{
    public readonly ObjectInput<JsonObject> Input;
    public readonly ObjectInput<string> Tag;
    public readonly ObjectInput<T> Object;

    public static bool IsValidGenericType => JsonTypeHelper.ValidObjectSetTypes.Contains(typeof(T));

    protected override JsonObject Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        if (input == null) return null;

        var tag = Tag.Evaluate(context);
        var obj = Object.Evaluate(context);
            
        return string.IsNullOrEmpty(tag) ? input : input.Add(tag, obj);
    }
}