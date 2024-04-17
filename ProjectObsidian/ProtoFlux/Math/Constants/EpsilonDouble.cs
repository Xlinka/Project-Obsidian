using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Constants;

[NodeCategory("Obsidian/Math/Constants")]
[NodeName("Epsilon Double")]
public class EpsilonDouble : ValueFunctionNode<ExecutionContext, double>
{
    protected override double Compute(ExecutionContext context)
    {
        return MathX.DOUBLE_EPSILON;
    }
}
