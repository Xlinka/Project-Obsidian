using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics;
using System;

public class CentripetalForceCalculationNode : ValueFunctionNode<ExecutionContext, float>
{
    public ValueInput<float> Mass;
    public ValueInput<float> Velocity;
    public ValueInput<float> Radius;

    protected override float Compute(ExecutionContext context)
    {
        float result = 0f;

        try
        {
            float m = Mass.Evaluate(context);
            float v = Velocity.Evaluate(context);
            float r = Radius.Evaluate(context);

            result = (m * v * v) / r;
        }
        catch (Exception ex)
        {
            // Log any exceptions without throwing them
            UniLog.Log($"Error in CentripetalForceCalculationNode.Compute: {ex.Message}");
        }

        return result;
    }
}
