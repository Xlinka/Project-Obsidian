using System;
using Elements.Core;
using ProtoFlux.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [NodeCategory("Obsidian/Math/Physics")]
    public class DragCalculationNode : ValueFunctionNode<ExecutionContext, float3>
    {
        public ValueInput<float> FluidDensity;        // rho
        public ValueInput<float3> ObjectVelocity;     // v
        public ValueInput<float> DragCoefficient;     // Cd
        public ValueInput<float> CrossSectionalArea;  // A

        protected override float3 Compute(ExecutionContext context)
        {
            try
            {
<<<<<<< HEAD
                float rho = FluidDensity.Evaluate(context);
                float3 v = ObjectVelocity.Evaluate(context);
                float Cd = DragCoefficient.Evaluate(context);
                float A = CrossSectionalArea.Evaluate(context);
=======
                var rho = FluidDensity.ReadValue(context);
                var v = ObjectVelocity.ReadValue(context);
                var Cd = DragCoefficient.ReadValue(context);
                var A = CrossSectionalArea.ReadValue(context);
>>>>>>> 1f3689c5903a431368bec8a85948d2884a1ffc17

                var vMagnitudeSquared = MathX.Dot(v, v);
                var dragDirection = Normalize(v);
                var dragForce = -0.5f * rho * vMagnitudeSquared * Cd * A * dragDirection;

                return dragForce;
            }
            catch (Exception ex)
            {
                // Log any exceptions without throwing them
                UniLog.Log($"Error in DragCalculationNode.Compute: {ex.Message}");
            }

            return float3.Zero;
        }
<<<<<<< HEAD

        // Define Normalize function since MathX doesn't have one
        private float3 Normalize(float3 vector)
        {
            float magnitude = MathX.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
            if (magnitude == 0)
                return float3.Zero;  // Handle zero magnitude case
            return new float3(vector.x / magnitude, vector.y / magnitude, vector.z / magnitude);
=======
        
        //this was done since Mathx doesnt have a normalise function why 
        private static float3 Normalize(float3 vector)
        {
            var magnitude = MathX.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
            return magnitude == 0 ? new float3(0) : // Handle zero magnitude case
                new float3(vector.x / magnitude, vector.y / magnitude, vector.z / magnitude);
>>>>>>> 1f3689c5903a431368bec8a85948d2884a1ffc17
        }
    }
}
