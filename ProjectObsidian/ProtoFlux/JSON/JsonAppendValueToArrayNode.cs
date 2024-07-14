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
[GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
    typeof(ulong), typeof(float), typeof(double))]
public class JsonAppendValueToArrayNode<T> : ObjectFunctionNode<FrooxEngineContext, JsonArray> where T : unmanaged
{
    public readonly ObjectInput<JsonArray> Array;
    public readonly ValueInput<T> Object;
    
    public static bool IsValidGenericType => JsonTypeHelper.ValidValueTypes.Contains(typeof(T));
    protected override JsonArray Compute(FrooxEngineContext context)
    {
        var array = Array.Evaluate(context);
        var obj = Object.Evaluate(context);
        return array?.Append(obj);
    }
}