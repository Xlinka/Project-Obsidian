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
[GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
    typeof(ulong), typeof(float), typeof(double))]
public class JsonQuickGetValueFromObjectNode<T> : ValueFunctionNode<FrooxEngineContext, T> where T : unmanaged
{
    public readonly ObjectInput<string> Input;
    public readonly ObjectInput<string> Tag;
    public static bool IsValidGenericType => JsonTypeHelper.ValidValueGetTypes.Contains(typeof(T));
    protected override T Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        var tag = Tag.Evaluate(context);
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(tag))
            return default;
        var from = JsonObject.FromString(input);
        return from?.GetValue<T>(tag) ?? default;
    }
}
