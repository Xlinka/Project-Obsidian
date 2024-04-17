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
    public class JsonQuickGetFromObjectNode<T> : ObjectFunctionNode<ExecutionContext, T>
    {
        public readonly ObjectInput<string> Input;
        public readonly ObjectInput<string> Tag;
        protected override T Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            var tag = Tag.Evaluate(context);
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(tag))
                return default;

            try
            {
                var inputObject = JObject.Parse(input);
                return inputObject[tag].Value<T>() ?? default;
            }
            catch
            {
                // In case of parsing error or if the tag is not found
                return default;
            }
        }
    }
}
