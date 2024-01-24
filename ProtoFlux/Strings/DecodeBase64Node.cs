using System;
using System.Text;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("ProtoFlux/Obsidian/String")]
    public class DecodeBase64Node : ObjectFunctionNode<ExecutionContext, string>
    {
        public readonly ObjectInput<string> Input;

        protected override string Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            if (string.IsNullOrEmpty(input)) return null;

            try
            {
                byte[] base64EncodedBytes = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch
            {
                // Return null or handle the error as required
                return null;
            }
        }
    }
}
