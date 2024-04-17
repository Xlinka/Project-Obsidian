using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Random;

[NodeCategory("Obsidian/Math/Random")]
[NodeName("Random Euler Angles")]
[ContinuouslyChanging]

public class RandomEulerAngles : ValueFunctionNode<ExecutionContext, float3>
{
    public ValueInput<float> minPitch;
    public ValueInput<float> maxPitch;
    public ValueInput<float> minYaw;
    public ValueInput<float> maxYaw;
    public ValueInput<float> minRoll;
    public ValueInput<float> maxRoll;

    protected override float3 Compute(ExecutionContext context)
    {
        float pitch = RandomX.Range(minPitch.Evaluate(context), maxPitch.Evaluate(context));
        float yaw = RandomX.Range(minYaw.Evaluate(context), maxYaw.Evaluate(context));
        float roll = RandomX.Range(minRoll.Evaluate(context), maxRoll.Evaluate(context));

        return new float3(pitch, yaw, roll);
    }
}
