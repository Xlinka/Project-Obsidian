using System;
using FrooxEngine;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(JToken), typeof(JObject), typeof(JArray))]
public class JsonToStringNode<T> : ObjectFunctionNode<ExecutionContext, string> where T : JToken
{
    public readonly ObjectInput<T> Input;

    protected override string Compute(ExecutionContext context)
    {
        var input = Input.Evaluate(context);
        return input?.ToString();
    }
}