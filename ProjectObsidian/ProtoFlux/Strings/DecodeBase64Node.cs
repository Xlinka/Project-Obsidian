using System;
using System.Text;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Strings
{
    [NodeCategory("Obsidian/String")]
    public class DecodeBase64Node : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public readonly ObjectInput<string> Input;

        protected override string Compute(FrooxEngineContext context)
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
