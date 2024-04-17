using System;
using System.Security.Cryptography;
using System.Text;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("Obsidian/String")]
    public class EncodeSha256Node : ObjectFunctionNode<ExecutionContext, string>
    {
        private static readonly SHA256 SHA = SHA256.Create();
        public readonly ObjectInput<string> Input;



        protected override string Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            if (string.IsNullOrEmpty(input))
                return null;

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
