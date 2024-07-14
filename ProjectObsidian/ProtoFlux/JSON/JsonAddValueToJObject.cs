using System;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeName("Add To Object")]
    [NodeCategory("Obsidian/Json")]
    [GenericTypes(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
                  typeof(long), typeof(ulong), typeof(float), typeof(double))]
    public class JsonAddValueToJObject<T> : ObjectFunctionNode<FrooxEngineContext, JObject> where T : unmanaged
    {
        public readonly ObjectInput<JObject> Input;
        public readonly ObjectInput<string> Tag;
        public readonly ValueInput<T> Value;

        protected override JObject Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            if (input == null) return null;

            var tag = Tag.Evaluate(context);
            var value = Value.Evaluate(context);
            if (string.IsNullOrEmpty(tag)) return input;

            var in2 = (JObject)input.DeepClone();
            in2[tag] = new JValue(value);
            return in2;
        }
    }
}
