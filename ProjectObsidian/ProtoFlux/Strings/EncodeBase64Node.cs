using System;
using System.Text;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("Obsidian/String")]
    public class EncodeBase64Node : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public readonly ObjectInput<string> Input;

        protected override string Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            return string.IsNullOrEmpty(input) ? null : Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }
    }
}
