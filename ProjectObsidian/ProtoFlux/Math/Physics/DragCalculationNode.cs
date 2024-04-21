using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using System;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [NodeCategory("Obsidian/Math/Physics")]
    public class DragCalculationNode : ValueFunctionNode<FrooxEngineContext, float3>
    {
        public ValueInput<float> FluidDensity;        // rho
        public ValueInput<float3> ObjectVelocity;     // v
        public ValueInput<float> DragCoefficient;     // Cd
        public ValueInput<float> CrossSectionalArea;  // A
        
        protected override float3 Compute(FrooxEngineContext context)
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

        private float3 Normalize(float3 vector)
        {
            float magnitude = MathX.Sqrt(MathX.Dot(vector, vector));
            return magnitude > 0 ? vector / magnitude : float3.Zero;
        }
    }
}
