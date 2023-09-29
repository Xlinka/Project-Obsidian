using System;
using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using System;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [NodeCategory("Obsidian/Math/Physics")]
    public class CentripetalForceCalculationNode : ValueFunctionNode<ExecutionContext, float>
    {
        public ValueArgument<float> Mass;           // Mass of the object
        public ValueArgument<float> Velocity;       // Tangential velocity of the object
        public ValueArgument<float> Radius;         // Radius of the circular path

        protected override float Compute(ExecutionContext context)
        {
            try
            {
                float m = Mass.ReadValue<float>(context);
                float v = Velocity.ReadValue<float>(context);
                float r = Radius.ReadValue<float>(context);

                // Debugging statements
                UniLog.Log($"Mass: {m}");
                UniLog.Log($"Velocity: {v}");
                UniLog.Log($"Radius: {r}");

                float result = (m * v * v) / r;

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
        }
    }
}
