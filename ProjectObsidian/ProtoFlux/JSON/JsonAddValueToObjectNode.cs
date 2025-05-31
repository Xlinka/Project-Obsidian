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
[GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
    typeof(long), typeof(ulong), typeof(float), typeof(double))]
public class JsonAddValueToObjectNode<T> : ObjectFunctionNode<FrooxEngineContext, JsonObject> where T : unmanaged
{
    public readonly ObjectInput<JsonObject> Input;
    public readonly ObjectInput<string> Tag;
    public readonly ValueInput<T> Object;

    public static bool IsValidGenericType => JsonTypeHelper.ValidValueTypes.Contains(typeof(T));

    protected override JsonObject Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        if (input == null) return null;

        var tag = Tag.Evaluate(context);
        var obj = Object.Evaluate(context);

        return string.IsNullOrEmpty(tag) ? input : input.Add(tag, obj);
    }
}
