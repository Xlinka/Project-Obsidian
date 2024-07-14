using System;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    [GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
                  typeof(ulong), typeof(float), typeof(double))]
    public class JsonAppendValueToJArray<T> : ObjectFunctionNode<FrooxEngineContext, JArray> where T : unmanaged
    {
        public readonly ObjectInput<JArray> Array;
        public readonly ValueInput<T> Value;
        protected override JArray Compute(FrooxEngineContext context)
        {
            var array = Array.Evaluate(context);
            var value = Value.Evaluate(context);
            if (array == null) return null;

            try
            {
                var output = (JArray)array.DeepClone();
                output.Add(new JValue(value));
                return output;
            }
            catch
            {
                return null;
            }
        }
    }
}
