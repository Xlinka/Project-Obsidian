using System;
using System.Security.Cryptography;
using System.Text;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("ProtoFlux/Obsidian/String")]
    public class EncodeMD5Node : ObjectFunctionNode<ExecutionContext, string>
    {
        private static readonly MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
        public readonly ObjectInput<string> Input;

        protected override string Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            if (string.IsNullOrEmpty(input))
                return null;

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
