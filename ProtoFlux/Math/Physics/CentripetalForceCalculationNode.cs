using System;
using Elements.Core;
using ProtoFlux.Core;

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
        }
    }
}
