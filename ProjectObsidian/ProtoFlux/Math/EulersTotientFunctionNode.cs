using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeCategory("Obsidian/Math")]
    [NodeName("EulersTotientFunction")]
    public class EulersTotientFunctionNode : ValueFunctionNode<FrooxEngineContext, int>
    {
        public ValueInput<int> Input;

        protected override int Compute(FrooxEngineContext context)
        {
            var result = Input.Evaluate(context);
            var inputCopy = result;
            for (var p = 2; p * p <= inputCopy; ++p)
            {
                if (inputCopy % p != 0) continue;
                while (inputCopy % p == 0) inputCopy /= p;
                result -= result / p;
            }
            if (inputCopy > 1) result -= result / inputCopy;
            return result;
        }
    }
}
