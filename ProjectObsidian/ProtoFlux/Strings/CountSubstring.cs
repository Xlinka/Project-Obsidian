using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Strings
{
    [NodeCategory("Obsidian/String")]
    public class CountSubstringNode : ValueFunctionNode<FrooxEngineContext, int>
    {
        public readonly ObjectInput<string> String;
        public readonly ObjectInput<string> Pattern;

        protected override int Compute(FrooxEngineContext context)
        {
            var str = String.Evaluate(context);
            var pattern = Pattern.Evaluate(context);

            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(pattern))
            {
                return 0;
            }

            return (str.Length - str.Replace(pattern, "").Length) / pattern.Length;
        }
    }
}
