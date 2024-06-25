using System;
using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeCategory("Obsidian/Math")]
    [NodeName("Fibonacci")]
    public class FibonacciNode : ValueFunctionNode<FrooxEngineContext, int>
    {
        public ValueInput<int> Input;

        protected override int Compute(FrooxEngineContext context)
        {
            int n = Input.Evaluate(context);
            return Fibonacci(n);
        }

        private int Fibonacci(int n)
        {
            if (n < 0)
                throw new ArgumentException("Negative numbers are not allowed.");
            if (n == 0)
                return 0;
            if (n == 1)
                return 1;

            int a = 0, b = 1, temp;
            for (int i = 2; i <= n; i++)
            {
                temp = a + b;
                a = b;
                b = temp;
            }
            return b;
        }
    }
}
