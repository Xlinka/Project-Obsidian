using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Strings
{
    [NodeCategory("Obsidian/String")]
    public class ReplaceIndexRangeNode : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public readonly ObjectInput<string> InputString;
        public readonly ObjectInput<string> Replacement;
        public readonly ValueInput<int> StartIndex;
        public readonly ValueInput<int> Length;

        protected override string Compute(FrooxEngineContext context)
        {
            var input = InputString.Evaluate(context);
            if (string.IsNullOrEmpty(input))
                return input;

            var replacement = Replacement.Evaluate(context) ?? "";
            var startIndex = StartIndex.Evaluate(context);
            var length = Length.Evaluate(context);

            if (startIndex < 0) startIndex = 0;
            if (startIndex > input.Length) startIndex = input.Length;

            if (length < 0) length = 0;
            if (startIndex + length > input.Length) length = input.Length - startIndex;

            // Replace the specified range
            return input.Substring(0, startIndex) + replacement + input.Substring(startIndex + length);
        }
    }
}