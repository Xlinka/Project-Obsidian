<<<<<<< HEAD
﻿using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics;
using System;
=======
﻿using System;
using Elements.Core;
using ProtoFlux.Core;
>>>>>>> 1f3689c5903a431368bec8a85948d2884a1ffc17

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
<<<<<<< HEAD
            float m = Mass.Evaluate(context);
            float v = Velocity.Evaluate(context);
            float r = Radius.Evaluate(context);

            result = (m * v * v) / r;
=======
            try
            {
                var m = Mass.ReadValue(context);
                var v = Velocity.ReadValue(context);
                var r = Radius.ReadValue(context);

                // Debugging statements
                UniLog.Log($"Mass: {m}");
                UniLog.Log($"Velocity: {v}");
                UniLog.Log($"Radius: {r}");

                var result = (m * v * v) / r;

                // Debugging the result
                UniLog.Log($"Result: {result}");

                return result;
            }
            catch (Exception ex)
            {
                // Log any exceptions
                UniLog.Log($"Error in CentripetalForceCalculationNode.Compute: {ex.Message}");
                throw;
            }
>>>>>>> 1f3689c5903a431368bec8a85948d2884a1ffc17
        }
        catch (Exception ex)
        {
            // Log any exceptions without throwing them
            UniLog.Log($"Error in CentripetalForceCalculationNode.Compute: {ex.Message}");
        }

        return result;
    }
}
