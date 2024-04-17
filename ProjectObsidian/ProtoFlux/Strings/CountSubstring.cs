using System;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("Obsidian/String")]
    public class CountSubstringNode : ValueFunctionNode<ExecutionContext, int>
    {
        public readonly ObjectInput<string> String;
        public readonly ObjectInput<string> Pattern;

        protected override int Compute(ExecutionContext context)
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
