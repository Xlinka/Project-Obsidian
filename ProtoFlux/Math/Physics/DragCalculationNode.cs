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
        public ValueArgument<float> FluidDensity;        // rho
        public ValueArgument<float3> ObjectVelocity;     // v
        public ValueArgument<float> DragCoefficient;     // Cd
        public ValueArgument<float> CrossSectionalArea;  // A

        protected override float3 Compute(ExecutionContext context)
        {
            try
            {
                float rho = FluidDensity.ReadValue<float>(context);
                float3 v = ObjectVelocity.ReadValue<float3>(context);
                float Cd = DragCoefficient.ReadValue<float>(context);
                float A = CrossSectionalArea.ReadValue<float>(context);

                float vMagnitudeSquared = MathX.Dot(v, v);
                float3 dragDirection = Normalize(v);
                float3 dragForce = -0.5f * rho * vMagnitudeSquared * Cd * A * dragDirection;

                return dragForce;
            }
            catch (Exception ex)
            {
                UniLog.Log($"Error in DragCalculation.Compute: {ex.Message}");
                throw;
            }
        }
        
        //this was done since Mathx doesnt have a normalise function why 
        private float3 Normalize(float3 vector)
        {
            float magnitude = MathX.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
            if (magnitude == 0)
                return new float3(0, 0, 0);  // Handle zero magnitude case
            return new float3(vector.x / magnitude, vector.y / magnitude, vector.z / magnitude);
        }
    }
}
