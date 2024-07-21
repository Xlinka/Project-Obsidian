using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Obsidian;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Random
{
    [NodeCategory("Obsidian/Math/Random")]
    [ContinuouslyChanging]
    public class RandomFloatQ : ValueFunctionNode<ExecutionContext, floatQ>
    {
        public readonly ValueInput<floatQ> Min;
        public readonly ValueInput<floatQ> Max;

        protected override floatQ Compute(ExecutionContext context)
        {
            floatQ min = Min.Evaluate(context);
            floatQ max = Max.Evaluate(context);

            floatQ randomQuat = RandomXExtensions.Range(min, max);
            randomQuat = randomQuat.Normalized;

            return randomQuat;
        }
    }
}