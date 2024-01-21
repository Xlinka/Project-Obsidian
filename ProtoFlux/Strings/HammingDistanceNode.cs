using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Strings
{
    [NodeCategory("ProtoFlux/Obsidian/String")]
    public class HammingDistanceNode : ObjectFunctionNode<FrooxEngineContext, int?>
    {
        public readonly ObjectInput<string> String1;
        public readonly ObjectInput<string> String2;

        protected override int? Compute(FrooxEngineContext context)
        {
            var string1 = String1.Evaluate(context);
            var string2 = String2.Evaluate(context);
            if (string1 == null || string2 == null || string1.Length != string2.Length)
                return null;

            var count = 0;
            for (var i = 0; i < string1.Length; i++)
                if (string1[i] != string2[i])
                    count++;
            return count;
        }
    }
}
