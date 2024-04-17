using System;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonRemoveFromArrayNode : ObjectFunctionNode<ExecutionContext, JArray>
    {
        public readonly ObjectInput<JArray> Array;
        public readonly ObjectInput<int> Index;


        protected override JArray Compute(ExecutionContext context)
        {
            var array = Array.Evaluate(context);
            var index = Index.Evaluate(context);
            if (array == null || index < 0 || index >= array.Count)
                return null;

            try
            {
                var output = (JArray)array.DeepClone();
                output.RemoveAt(index);
                return output;
            }
            catch
            {
                // In case of an error, return null
                return null;
            }
        }
    }
}
