using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Constants;

[NodeCategory("Obsidian/Math/Constants")]
[NodeName("Epsilon Float")]
public class EpsilonFloat : ValueFunctionNode<FrooxEngineContext, float>
{
    protected override float Compute(FrooxEngineContext context)
    {
        return MathX.FLOAT_EPSILON;
    }
}
