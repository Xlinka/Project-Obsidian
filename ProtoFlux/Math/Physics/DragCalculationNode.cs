using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using System;

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
                float rho = FluidDensity.Evaluate(context);
                float3 v = ObjectVelocity.Evaluate(context);
                float Cd = DragCoefficient.Evaluate(context);
                float A = CrossSectionalArea.Evaluate(context);

                float vMagnitudeSquared = MathX.Dot(v, v);
                float3 dragDirection = Normalize(v);
                float3 dragForce = -0.5f * rho * vMagnitudeSquared * Cd * A * dragDirection;

                return dragForce;
            }
            catch (Exception ex)
            {
                // Log any exceptions without throwing them
                UniLog.Log($"Error in DragCalculationNode.Compute: {ex.Message}");
            }

            return float3.Zero;
        }

        // Define Normalize function since MathX doesn't have one
        private float3 Normalize(float3 vector)
        {
            float magnitude = MathX.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
            if (magnitude == 0)
                return float3.Zero;  // Handle zero magnitude case
            return new float3(vector.x / magnitude, vector.y / magnitude, vector.z / magnitude);
        }
    }
}
