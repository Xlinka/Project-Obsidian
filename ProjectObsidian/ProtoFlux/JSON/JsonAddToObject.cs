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
                  typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(string), typeof(Uri),
                  typeof(JToken), typeof(JObject), typeof(JArray))]
    public class JsonAddToObjectNode<T> : ObjectFunctionNode<FrooxEngineContext, JObject>
    {
        public readonly ObjectInput<JObject> Input;
        public readonly ObjectInput<string> Tag;
        public readonly ObjectInput<T> Object;

        protected override JObject Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            if (input == null) return null;

            var tag = Tag.Evaluate(context);
            var obj = Object.Evaluate(context);
            if (string.IsNullOrEmpty(tag) || obj == null) return input;

            var in2 = (JObject)input.DeepClone();
            in2[tag] = obj switch
            {
                JArray jArray => jArray,
                JObject jObject => jObject,
                JToken token => token,
                _ => new JValue(obj)
            };
            return in2;
        }
    }
}
