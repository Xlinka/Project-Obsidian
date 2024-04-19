using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(JToken), typeof(JObject), typeof(JArray))]
public class JsonToStringNode<T> : ObjectFunctionNode<FrooxEngineContext, string>
{
    public readonly ObjectInput<T> Input;

    protected override string Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        return input?.ToString();
    }
}