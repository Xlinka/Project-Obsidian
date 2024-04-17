using System;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    [GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
                  typeof(ulong), typeof(float), typeof(double), typeof(string), typeof(Uri), typeof(JToken), typeof(JObject),
                  typeof(JArray))]
    public class JsonInsertToArrayNode<T> : ObjectFunctionNode<ExecutionContext, JArray>
    {
        public readonly ObjectInput<JArray> Array;
        public readonly ObjectInput<T> Object;
        public readonly ObjectInput<int> Index;

        protected override JArray Compute(ExecutionContext context)
        {
            var array = Array.Evaluate(context);
            var obj = Object.Evaluate(context);
            var index = Index.Evaluate(context);
            if (array == null || obj == null || index < 0 || index > array.Count)
                return null;

            try
            {
                var output = (JArray)array.DeepClone();
                JToken token = obj is JToken jToken ? jToken : new JValue(obj);
                output.Insert(index, token);
                return output;
            }
            catch
            {
                return null;
            }
        }
    }
}
