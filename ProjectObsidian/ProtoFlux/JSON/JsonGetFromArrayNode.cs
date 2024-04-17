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
    public class JsonGetFromArrayNode<T> : ObjectFunctionNode<ExecutionContext, T>
    {
        public readonly ObjectInput<JArray> Input;
        public readonly ObjectInput<int> Index;

   
        protected override T Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            var index = Index.Evaluate(context);
            if (input == null || index < 0 || index >= input.Count)
                return default;

            try
            {
                return input[index].Value<T>();
            }
            catch
            {
                return default;
            }
        }
    }
}
