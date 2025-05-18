using System;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using Obsidian.Elements;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeName("JsonToken From String")]
[NodeCategory("Obsidian/Json")]
public class JsonParseStringTokenNode : ObjectFunctionNode<FrooxEngineContext, JsonToken>
{
    public readonly ObjectInput<string> Input;

    protected override JsonToken Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        return string.IsNullOrEmpty(input) ? null : JsonToken.FromString(input);
    }
}