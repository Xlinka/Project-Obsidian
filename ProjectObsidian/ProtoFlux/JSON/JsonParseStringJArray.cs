using System;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonParseStringJArray : ObjectFunctionNode<FrooxEngineContext, JArray>
    {
        public readonly ObjectInput<string> Input;

        protected override JArray Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            if (string.IsNullOrEmpty(input))
                return null;

            try
            {
                var output = JArray.Parse(input);
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
