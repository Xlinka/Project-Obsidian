using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using FrooxEngine;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Random;

[NodeCategory("Obsidian/Math/Random")]
[NodeName("Random Bool3")]
[ContinuouslyChanging]
public class RandomBool3 : ValueFunctionNode<ExecutionContext, bool3>
{
    public ValueInput<float3> Chance;

    protected override bool3 Compute(ExecutionContext context)
    {
        var chance = Chance.Evaluate(context);
        return new bool3(RandomX.Chance(chance.x), RandomX.Chance(chance.y), RandomX.Chance(chance.z));
    }
}