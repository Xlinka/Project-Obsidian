using System;
using System.Linq;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using Obsidian.Elements;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeName("To String")]
[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(IJsonToken), typeof(JsonObject), typeof(JsonArray))]
public class JsonToStringNode<T> : ObjectFunctionNode<FrooxEngineContext, string>
{
    public readonly ObjectInput<T> Input;
    public static bool IsValidGenericType => JsonTypeHelper.JsonTokens.Contains(typeof(T));
    protected override string Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        return input?.ToString();
    }
}