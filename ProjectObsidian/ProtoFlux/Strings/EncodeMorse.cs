using System;
using System.Text;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Obsidian;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("Obsidian/String")]
    public class EncodeMorseNode : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public readonly ObjectInput<string> Input;

        protected override string Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.ToUpperInvariant();
            var result = new StringBuilder();
            foreach (var c in input)
            {
                if (c == ' ')
                {
                    result.Append("/ ");
                    continue;
                }
                if (NodeExtensions.CharToMorse.TryGetValue(c, out var morseChar))
                    result.Append(morseChar + " ");
            }
            return result.ToString().TrimEnd();
        }
    }
}
