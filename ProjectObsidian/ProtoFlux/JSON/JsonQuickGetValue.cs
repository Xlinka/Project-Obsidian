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
    public class JsonQuickGetValue<T> : ValueFunctionNode<FrooxEngineContext, T> where T : unmanaged
    {
        public readonly ObjectInput<string> Input;
        public readonly ObjectInput<string> Tag;
        protected override T Compute(FrooxEngineContext context)
        {
            string input = Input.Evaluate(context);
            string tag = Tag.Evaluate(context);
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(tag))
                return default;

            try
            {
                var inputObject = JObject.Parse(input);
                return inputObject[tag].Value<T>();
            }
            catch
            {
                // In case of parsing error or if the tag is not found
                return default;
            }
        }
    }
}