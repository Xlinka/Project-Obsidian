using System;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("ProtoFlux/Obsidian/Json")]
    public class JsonRemoveFromObjectNode : ObjectFunctionNode<ExecutionContext, JObject>
    {
        public readonly ObjectInput<JObject> Input;
        public readonly ObjectInput<string> Tag;

  
        protected override JObject Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            if (input == null) return null;

            var tag = Tag.Evaluate(context);
            if (string.IsNullOrEmpty(tag)) return input;

            var output = (JObject)input.DeepClone();
            output.Remove(tag);
            return output;
        }
    }
}
