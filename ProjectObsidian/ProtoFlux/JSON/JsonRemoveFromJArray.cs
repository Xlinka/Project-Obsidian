using System;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonRemoveFromJArray : ObjectFunctionNode<FrooxEngineContext, JArray>
    {
        public readonly ObjectInput<JArray> Array;
        public readonly ValueInput<int> Index;

        protected override JArray Compute(FrooxEngineContext context)
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
