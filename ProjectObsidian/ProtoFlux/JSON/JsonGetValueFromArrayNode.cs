using System.Linq;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[NodeCategory("Obsidian/Json")]
[GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
    typeof(ulong), typeof(float), typeof(double))]
public class JsonGetValueFromArrayNode<T> : ValueFunctionNode<FrooxEngineContext, T> where T : unmanaged
{
    public readonly ObjectInput<JsonArray> Input;
    public readonly ObjectInput<int> Index;
    public static bool IsValidGenericType => JsonTypeHelper.ValidValueGetTypes.Contains(typeof(T));
    protected override T Compute(FrooxEngineContext context)
    {
        var input = Input.Evaluate(context);
        var index = Index.Evaluate(context);
        if (input == null || index < 0 || index >= input.Count)
            return default;

        return input.GetValue<T>(index);
    }
}