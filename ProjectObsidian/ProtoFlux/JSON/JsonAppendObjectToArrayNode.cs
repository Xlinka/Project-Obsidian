using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeName("Append To Array")]
[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(string), typeof(Uri), typeof(JsonToken), typeof(JsonObject), typeof(JsonArray), typeof(IJsonToken))]
public class JsonAppendObjectToArrayNode<T> : ObjectFunctionNode<FrooxEngineContext, JsonArray> where T : class
{
    public readonly ObjectInput<JsonArray> Array;
    public readonly ObjectInput<T> Object;
        
    public static bool IsValidGenericType => JsonTypeHelper.ValidObjectSetTypes.Contains(typeof(T));
    protected override JsonArray Compute(FrooxEngineContext context)
    {
        var array = Array.Evaluate(context);
        var obj = Object.Evaluate(context);
        return array?.Append(obj);
    }
}