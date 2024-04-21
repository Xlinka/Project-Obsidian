using System;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonParseStringNode : ObjectFunctionNode<FrooxEngineContext, JObject>
    {
        public readonly ObjectInput<string> Input;

   
        protected override JObject Compute(FrooxEngineContext context)
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
