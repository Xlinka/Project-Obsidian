using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

[NodeCategory("Protoflux/Obsidian/Math/Constants")]
[NodeName("Epsilon Float")]
public class EpsilonFloat : ValueFunctionNode<ExecutionContext, float>
{
    protected override float Compute(ExecutionContext context)
    {
        return MathX.FLOAT_EPSILON;
    }
}
