using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeCategory("Obsidian/Math")]
    [NodeName("IsPrimeNumber")]
    public class IsPrimeNumberNode : ValueFunctionNode<FrooxEngineContext, bool>
    {
        public ValueInput<int> Input;

        protected override bool Compute(FrooxEngineContext context)
        {
            var num = Input.Evaluate(context);
            switch (num)
            {
                case < 2:
                    return false;
                case 2:
                    return true;
                default:
                    {
                        if (num % 2 == 0) return false;
                        break;
                    }
            }
            double sqrtNum = MathX.Sqrt(num);
            for (var i = 3; i <= sqrtNum; i += 2)
            {
                if (num % i == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
