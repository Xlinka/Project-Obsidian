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

[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(string), typeof(Uri), typeof(JsonObject), typeof(JsonArray))]
public class JsonGetObjectFromArrayNode<T> : ObjectFunctionNode<FrooxEngineContext, T> where T : class
{
    public readonly ObjectInput<JsonArray> Input;
    public readonly ObjectInput<int> Index;
    public static bool IsValidGenericType => JsonTypeHelper.ValidObjectTypes.Contains(typeof(T));
    protected override T Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        var index = Index.Evaluate(context);
        if (input == null || index < 0 || index >= input.Count)
            return default;

        return input.GetObject<T>(index);
    }
}