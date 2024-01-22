using System;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine;
using Elements.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("ProtoFlux/Obsidian/Json")]
    [GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
                  typeof(ulong), typeof(float), typeof(double), typeof(string), typeof(Uri), typeof(JToken), typeof(JObject),
                  typeof(JArray))]
    public class JsonAppendToArrayNode<T> : ObjectFunctionNode<ExecutionContext, JArray>
    {
        public readonly ObjectInput<JArray> Array;
        public readonly ObjectInput<T> Object;
        protected override JArray Compute(ExecutionContext context)
        {
            var array = Array.Evaluate(context);
            var obj = Object.Evaluate(context);
            if (array == null || obj == null) return null;

            try
            {
                var output = (JArray)array.DeepClone();
                output.Add(obj switch
                {
                    JToken token => token,
                    _ => new JValue(obj)
                });
                return output;
            }
            catch
            {
                return null;
            }
        }
    }
}
