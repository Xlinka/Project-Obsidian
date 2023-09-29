using System;
using Elements.Core;
using ProtoFlux.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [NodeCategory("Obsidian/Math/Physics")]
    public class DragCalculationNode : ValueFunctionNode<ExecutionContext, float3>
    {
        public ValueArgument<float> FluidDensity;        // rho
        public ValueArgument<float3> ObjectVelocity;     // v
        public ValueArgument<float> DragCoefficient;     // Cd
        public ValueArgument<float> CrossSectionalArea;  // A

        protected override float3 Compute(ExecutionContext context)
        {
            try
            {
                var rho = FluidDensity.ReadValue(context);
                var v = ObjectVelocity.ReadValue(context);
                var Cd = DragCoefficient.ReadValue(context);
                var A = CrossSectionalArea.ReadValue(context);

                var vMagnitudeSquared = MathX.Dot(v, v);
                var dragDirection = Normalize(v);
                var dragForce = -0.5f * rho * vMagnitudeSquared * Cd * A * dragDirection;

                return dragForce;
            }
            catch (Exception ex)
            {
                UniLog.Log($"Error in DragCalculation.Compute: {ex.Message}");
                throw;
            }
        }
        
        //this was done since Mathx doesnt have a normalise function why 
        private static float3 Normalize(float3 vector)
        {
            var magnitude = MathX.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
            return magnitude == 0 ? new float3(0) : // Handle zero magnitude case
                new float3(vector.x / magnitude, vector.y / magnitude, vector.z / magnitude);
        }
    }
}
