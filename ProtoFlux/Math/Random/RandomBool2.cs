using System;
using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;

[NodeCategory("ProtoFlux/Obsidian/Math/Random")]
[ContinuouslyChanging]

public class RandomBool2 : ObjectFunctionNode<ExecutionContext, bool2>
{
    public ValueInput<float2> Chance;

    protected override bool2 Compute(ExecutionContext context)
    {
        float2 chance = Chance.Evaluate(context);
        bool2 result = new bool2(
            RandomX.Chance(chance.x),
            RandomX.Chance(chance.y)
        );
        return result;
    }
}
