using System;
using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Random
{
    [NodeCategory("Obsidian/Math/Random")]
    [NodeName("Random Character")]
    [ContinuouslyChanging]
    public class RandomCharacter : ValueFunctionNode<FrooxEngineContext, char>
    {
        public ValueInput<int> Start;
        public ValueInput<int> End;

        protected override char Compute(FrooxEngineContext context)
        {
            int start = MathX.Clamp(Start.Evaluate(context), 0, 25); 
            int end = MathX.Clamp(End.Evaluate(context), start, 25); 
            if (start == end)
            {
                return (char)('A' + start); 
            }

            int randomIndex = RandomX.Range(start, end + 1); 
            return (char)('A' + randomIndex);
        }
    }
}
