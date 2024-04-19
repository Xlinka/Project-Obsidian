using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Random;

[NodeCategory("Obsidian/Math/Random")]
[NodeName("Random Bool4")]
[ContinuouslyChanging]
public class RandomBool4 : ValueFunctionNode<FrooxEngineContext, bool4>
{
    public ValueInput<float4> Chance;

    protected override bool4 Compute(FrooxEngineContext context)
    {
        var chance = Chance.Evaluate(context);
        return new bool4(
            RandomX.Chance(chance.x),
            RandomX.Chance(chance.y),
            RandomX.Chance(chance.z),
            RandomX.Chance(chance.w)
        );
    }
}
