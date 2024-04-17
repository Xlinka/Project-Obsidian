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
    public class JsonGetFromObjectNode<T> : ObjectFunctionNode<ExecutionContext, T>
    {
        public readonly ObjectInput<JObject> Input;
        public readonly ObjectInput<string> Tag;

        protected override T Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            var tag = Tag.Evaluate(context);
            if (input == null || string.IsNullOrEmpty(tag))
                return default;

            try
            {
                return input[tag].Value<T>();
            }
            catch
            {
                return default;
            }
        }
    }
}
