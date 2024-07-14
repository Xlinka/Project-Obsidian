using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    [GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
                  typeof(ulong), typeof(float), typeof(double), typeof(string), typeof(Uri), 
                  typeof(IJsonToken), typeof(JsonObject), typeof(JsonArray))]
    public class JsonInsertToArrayNode<T> : ObjectFunctionNode<FrooxEngineContext, JsonArray>
    {
        public readonly ObjectInput<JsonArray> Array;
        public readonly ObjectInput<T> Object;
        public readonly ObjectInput<int> Index;
        public static bool IsValidGenericType => JsonTypeHelper.AllValidTypes.Contains(typeof(T));
        protected override JsonArray Compute(FrooxEngineContext context)
        {
            var array = Array.Evaluate(context);
            var obj = Object.Evaluate(context);
            var index = Index.Evaluate(context);
            if (array == null || index < 0 || index > array.Count)
                return null;

            return array.Insert(index, obj);
        }
    }
}
