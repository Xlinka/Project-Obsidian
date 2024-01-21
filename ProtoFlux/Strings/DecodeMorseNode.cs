using System;
using System.Text;
using FrooxEngine;
using Obsidian;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("ProtoFlux/Obsidian/String")]
    public class DecodeMorseNode : ObjectFunctionNode<ExecutionContext, string>
    {
        public readonly ObjectInput<string> Input;

        protected override string Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var words = input.Split('/');
            var result = new StringBuilder();
            foreach (var word in words)
            {
                var characters = word.Split(' ');
                if (string.IsNullOrWhiteSpace(word)) continue;

                foreach (var character in characters)
                {
                    if (string.IsNullOrWhiteSpace(character)) continue;
                    if (NodeExtensions.MorseToChar.TryGetValue(character, out var ch))
                        result.Append(ch);
                }
                result.Append(' ');
            }
            return result.ToString().Trim();
        }
    }
}
