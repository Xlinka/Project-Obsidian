using System;
using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeCategory("Obsidian/Math")]
    [NodeName("Factorial")]
    public class FactorialNode : ValueFunctionNode<FrooxEngineContext, int>
    {
        public ValueInput<int> Input;

        protected override int Compute(FrooxEngineContext context)
        {
            var input = Input.Evaluate(context);
            var fact = 1;
            var loop = MathX.Clamp(input, 0, 16);
            for (var i = 1; i <= loop; i++) fact *= i;
            return fact;
        }
    }
}
