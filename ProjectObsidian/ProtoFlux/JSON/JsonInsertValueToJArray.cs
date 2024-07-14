using System;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    [GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
                  typeof(ulong), typeof(float), typeof(double))]
    public class JsonInsertValueToJArray<T> : ObjectFunctionNode<FrooxEngineContext, JArray> where T : unmanaged
    {
        public readonly ObjectInput<JArray> Array;
        public readonly ValueInput<T> Value;
        public readonly ValueInput<int> Index;

        protected override JArray Compute(FrooxEngineContext context)
        {
            var array = Array.Evaluate(context);
            var value = Value.Evaluate(context);
            var index = Index.Evaluate(context);
            if (array == null || index < 0 || index > array.Count)
                return null;

            try
            {
                var output = (JArray)array.DeepClone();
                var token = new JValue(value);
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
