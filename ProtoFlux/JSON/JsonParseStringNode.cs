using System;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("ProtoFlux/Obsidian/Json")]
    public class JsonParseStringNode : ObjectFunctionNode<ExecutionContext, JObject>
    {
        public readonly ObjectInput<string> Input;

   
        protected override JObject Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            if (string.IsNullOrEmpty(input))
                return null;

            try
            {
                var output = JObject.Parse(input);
                return output;
            }
            catch
            {
                // In case of parsing error, return null
                return null;
            }
        }
    }
}
