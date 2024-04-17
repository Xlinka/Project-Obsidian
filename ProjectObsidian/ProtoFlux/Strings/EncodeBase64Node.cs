using System;
using System.Text;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("ProtoFlux/Obsidian/String")]
    public class EncodeBase64Node : ObjectFunctionNode<ExecutionContext, string>
    {
        public readonly ObjectInput<string> Input;

        protected override string Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            return string.IsNullOrEmpty(input) ? null : Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }
    }
}
