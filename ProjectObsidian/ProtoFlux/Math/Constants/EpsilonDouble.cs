using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Constants;

[NodeCategory("Obsidian/Math/Constants")]
[NodeName("Epsilon Double")]
public class EpsilonDouble : ValueFunctionNode<FrooxEngineContext, double>
{
    protected override double Compute(FrooxEngineContext context)
    {
        return MathX.DOUBLE_EPSILON;
    }
}
