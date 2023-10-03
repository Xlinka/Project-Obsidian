using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using System;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [NodeCategory("Obsidian/Math/Physics")]
    public class KineticFrictionNode : ValueFunctionNode<ExecutionContext, float3>
    {
        public ValueInput<float3> NormalForce; // Assuming it's a 3D force, change as needed
        public ValueInput<float> KineticFrictionCoefficient;
        public ValueOutput<float3> KineticFrictionalForce;

        protected override float3 Compute(ExecutionContext context)
        {
            float3 normal = NormalForce.Evaluate(context);
            float coefficient = KineticFrictionCoefficient.Evaluate(context);

            // Kinetic friction formula: f_kinetic = mu_kinetic * N
            float3 kineticFrictionalForce = coefficient * normal;

            return kineticFrictionalForce;
        }
    }
}
